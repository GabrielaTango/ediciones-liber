using BookstoreAPI.Models;
using BookstoreAPI.Models.Afip;
using QRCoder;
using System.Text;
using System.Text.Json;

namespace BookstoreAPI.Services.Afip
{
    public class AfipQrService : IAfipQrService
    {
        private readonly IAfipConfigProvider _configProvider;
        private readonly ILogger<AfipQrService> _logger;

        public AfipQrService(
            IAfipConfigProvider configProvider,
            ILogger<AfipQrService> logger)
        {
            _configProvider = configProvider;
            _logger = logger;
        }

        public async Task<string> GenerarQrBase64Async(Comprobante comprobante, Cliente cliente)
        {
            var qrBytes = await GenerarQrBytesAsync(comprobante, cliente);
            return Convert.ToBase64String(qrBytes);
        }

        public string GenerarQrBase64(Comprobante comprobante, Cliente cliente)
        {
            return GenerarQrBase64Async(comprobante, cliente).GetAwaiter().GetResult();
        }

        public byte[] GenerarQrBytes(Comprobante comprobante, Cliente cliente)
        {
            return GenerarQrBytesAsync(comprobante, cliente).GetAwaiter().GetResult();
        }

        public async Task<byte[]> GenerarQrBytesAsync(Comprobante comprobante, Cliente cliente)
        {
            try
            {
                var _config = await _configProvider.GetConfigAsync();
                // Construir URL del QR según especificaciones de AFIP
                var qrUrl = GenerarUrlQr(comprobante, cliente, _config);

                // Generar QR code
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(qrUrl, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);

                return qrCode.GetGraphic(20);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar código QR de AFIP");
                throw;
            }
        }

        private string GenerarUrlQr(Comprobante comprobante, Cliente cliente, AfipConfig _config)
        {
            try
            {
                // Parsear número de comprobante (formato: 00001-00000123)
                int puntoVenta = _config.PuntoVenta;
                long numeroComp = 0;

                if (!string.IsNullOrEmpty(comprobante.NumeroComprobante))
                {
                    var partes = comprobante.NumeroComprobante.Split('-');
                    if (partes.Length > 0 && int.TryParse(partes[0], out var ptoVta))
                    {
                        puntoVenta = ptoVta;
                    }
                    if (partes.Length > 1 && long.TryParse(partes[1], out var nroComp))
                    {
                        numeroComp = nroComp;
                    }
                }

                // Determinar tipo de documento
                int tipoDoc = DeterminarTipoDocumento(cliente.TipoDocumento);

                // Parsear número de documento de forma segura
                long nroDoc = 0;
                if (!string.IsNullOrEmpty(cliente.NroDocumento))
                {
                    var docLimpio = cliente.NroDocumento.Replace("-", "").Replace(".", "").Trim();
                    long.TryParse(docLimpio, out nroDoc);
                }

                // Determinar tipo de comprobante
                int tipoCmp = DeterminarTipoComprobante(cliente.CategoriaIva);

                // Parsear CAE de forma segura
                long codAutorizacion = 0;
                if (!string.IsNullOrEmpty(comprobante.CAE))
                {
                    long.TryParse(comprobante.CAE.Trim(), out codAutorizacion);
                }

                // Parsear CUIT de forma segura
                long cuit = 0;
                if (!string.IsNullOrEmpty(_config.CUIT))
                {
                    long.TryParse(_config.CUIT.Replace("-", ""), out cuit);
                }

                // Construir objeto JSON según especificaciones de AFIP
                var qrData = new
                {
                    ver = 1,
                    fecha = comprobante.Fecha.ToString("yyyy-MM-dd"),
                    cuit = cuit,
                    ptoVta = puntoVenta,
                    tipoCmp = tipoCmp,
                    nroCmp = numeroComp,
                    importe = comprobante.Total,
                    moneda = "PES",
                    ctz = 1,
                    tipoDocRec = tipoDoc,
                    nroDocRec = nroDoc,
                    tipoCodAut = "E", // E = CAE
                    codAut = codAutorizacion
                };

                // Serializar a JSON
                var jsonString = JsonSerializer.Serialize(qrData);

                // Convertir a base64
                var jsonBytes = Encoding.UTF8.GetBytes(jsonString);
                var base64 = Convert.ToBase64String(jsonBytes);

                // Construir URL final
                var baseUrl = _config.IsProduction
                    ? "https://www.afip.gob.ar/fe/qr/"
                    : "https://www.afip.gob.ar/fe/qr/"; // Mismo URL para homologación

                return $"{baseUrl}?p={base64}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar URL del QR");
                throw;
            }
        }

        private int DeterminarTipoComprobante(string? categoriaIva)
        {
            // Factura A = 1, Factura B = 6, Factura C = 11
            return categoriaIva?.ToUpper() switch
            {
                "RESPONSABLE INSCRIPTO" => 1,
                "MONOTRIBUTO" => 6,
                "CONSUMIDOR FINAL" => 11,
                "EXENTO" => 6,
                _ => 11
            };
        }

        private int DeterminarTipoDocumento(string? tipoDocumento)
        {
            return tipoDocumento?.ToUpper() switch
            {
                "CUIT" => 80,
                "CUIL" => 86,
                "DNI" => 96,
                "CDI" => 87,
                _ => 99 // Sin identificar
            };
        }
    }
}
