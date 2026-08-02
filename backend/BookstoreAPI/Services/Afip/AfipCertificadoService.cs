using BookstoreAPI.DTOs;
using BookstoreAPI.Models.Afip;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BookstoreAPI.Services.Afip
{
    public class AfipCertificadoService : IAfipCertificadoService
    {
        /// <summary>Días de anticipación con los que se avisa que el certificado está por vencer.</summary>
        private const int DiasAvisoVencimiento = 60;

        /// <summary>BOM (U+FEFF) que puede quedar al inicio del PEM guardado en la DB.</summary>
        private const char Bom = (char)0xFEFF;

        private readonly IAfipConfigProvider _configProvider;
        private readonly ILogger<AfipCertificadoService> _logger;

        public AfipCertificadoService(IAfipConfigProvider configProvider, ILogger<AfipCertificadoService> logger)
        {
            _configProvider = configProvider;
            _logger = logger;
        }

        public async Task<CertificadoEstadoDto> GetEstadoAsync()
        {
            try
            {
                var config = await _configProvider.GetConfigAsync();
                using var cert = LeerCertificado(config);

                if (cert == null)
                {
                    return new CertificadoEstadoDto
                    {
                        TieneCertificado = false,
                        Estado = "sin_certificado",
                        Mensaje = "No hay un certificado AFIP cargado"
                    };
                }

                // NotBefore/NotAfter vienen en hora local
                var vencimiento = cert.NotAfter;
                var diasRestantes = (int)Math.Floor((vencimiento.Date - DateTime.Today).TotalDays);

                var estado = diasRestantes < 0
                    ? "vencido"
                    : diasRestantes <= DiasAvisoVencimiento
                        ? "por_vencer"
                        : "vigente";

                return new CertificadoEstadoDto
                {
                    TieneCertificado = true,
                    Estado = estado,
                    FechaEmision = cert.NotBefore,
                    FechaVencimiento = vencimiento,
                    DiasRestantes = diasRestantes,
                    Subject = cert.Subject
                };
            }
            catch (Exception ex)
            {
                // El panel del dashboard nunca debe romperse por un problema con el certificado
                _logger.LogError(ex, "Error al leer el estado del certificado AFIP");
                return new CertificadoEstadoDto
                {
                    TieneCertificado = false,
                    Estado = "error",
                    Mensaje = "No se pudo leer el certificado: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Lee el certificado con la misma prioridad que AfipAuthService.LoadCertificate,
        /// pero sin la clave privada: para conocer el vencimiento alcanza con el CRT.
        /// </summary>
        private X509Certificate2? LeerCertificado(AfipConfig config)
        {
            // Prioridad 1: certificado desde la base de datos
            if (config.CrtBytes != null && config.CrtBytes.Length > 0)
                return CargarDesdeBytes(config.CrtBytes);

            // Prioridad 2: archivo CRT
            if (!string.IsNullOrEmpty(config.CrtPath) && File.Exists(config.CrtPath))
                return CargarDesdeBytes(File.ReadAllBytes(config.CrtPath));

            // Prioridad 3: PFX
            if (!string.IsNullOrEmpty(config.PfxPath) && File.Exists(config.PfxPath))
                return new X509Certificate2(config.PfxPath, config.PfxPassword, X509KeyStorageFlags.EphemeralKeySet);

            return null;
        }

        private static X509Certificate2 CargarDesdeBytes(byte[] bytes)
        {
            var texto = Encoding.UTF8.GetString(bytes).Trim().Trim(Bom);

            if (texto.Contains("-----BEGIN CERTIFICATE-----"))
                return X509Certificate2.CreateFromPem(texto.Replace("\r\n", "\n"));

            // Certificado en formato binario (DER)
            return new X509Certificate2(bytes);
        }
    }
}
