using BookstoreAPI.Models.Afip;
using BookstoreAPI.Repositories;
using System.Globalization;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace BookstoreAPI.Services.Afip
{
    public class AfipAuthService : IAfipAuthService
    {
        private readonly IAfipConfigProvider _configProvider;
        private readonly IConfiguracionRepository _configuracionRepo;
        private readonly ILogger<AfipAuthService> _logger;
        private static AfipTicketAcceso? _cachedTicket;
        private static readonly object _lock = new object();
        private const string TA_DB_KEY = "Afip_TA";

        public AfipAuthService(IAfipConfigProvider configProvider, IConfiguracionRepository configuracionRepo, ILogger<AfipAuthService> logger)
        {
            _configProvider = configProvider;
            _configuracionRepo = configuracionRepo;
            _logger = logger;
        }

        public async Task<AfipTicketAcceso> GetTicketAccesoAsync()
        {
            // Intentar cargar desde DB si no hay cache
            if (_cachedTicket == null)
            {
                try
                {
                    var taXml = await _configuracionRepo.GetValueAsync(TA_DB_KEY);
                    if (!string.IsNullOrEmpty(taXml))
                    {
                        _cachedTicket = ParsearTaXml(taXml);
                        _logger.LogInformation("TA cargado desde base de datos. Expira: {ExpirationTime}", _cachedTicket.ExpirationTime);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo cargar TA desde base de datos");
                }
            }

            lock (_lock)
            {
                if (_cachedTicket != null && _cachedTicket.IsValid())
                {
                    _logger.LogInformation("Usando ticket de acceso cacheado");
                    return _cachedTicket;
                }
            }

            var config = await _configProvider.GetConfigAsync();
            _logger.LogInformation("Solicitando nuevo ticket de acceso a AFIP");

            try
            {
                var loginTicketRequest = GenerateLoginTicketRequest(config);
                _logger.LogInformation("LoginTicketRequest generado (longitud: {Length})", loginTicketRequest.Length);

                var cert = LoadCertificate(config);
                _logger.LogInformation("Certificado cargado. Subject: {Subject}, Expira: {NotAfter}, HasPrivateKey: {HasPrivateKey}",
                    cert.Subject, cert.NotAfter, cert.HasPrivateKey);

                Encoding encodedMsg = Encoding.UTF8;
                byte[] msgBytes = encodedMsg.GetBytes(loginTicketRequest);
                byte[] encodedSignedCms = FirmaBytesMensaje(msgBytes, cert);
                var cmsFirmadoBase64 = Convert.ToBase64String(encodedSignedCms);

                _logger.LogInformation("LoginTicketRequest firmado exitosamente. Tamaño base64: {Size} caracteres", cmsFirmadoBase64.Length);

                var (ticket, credentialsXml) = await SendToWSAAAsync(cmsFirmadoBase64, config);

                // Guardar el XML crudo de credenciales en la DB
                try
                {
                    await _configuracionRepo.SetValueAsync(TA_DB_KEY, credentialsXml);
                    _logger.LogInformation("TA guardado en base de datos");
                }
                catch (Exception saveEx)
                {
                    _logger.LogWarning(saveEx, "No se pudo guardar TA en base de datos");
                }

                lock (_lock)
                {
                    _cachedTicket = ticket;
                }

                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ticket de acceso de AFIP");
                throw;
            }
        }

        private AfipTicketAcceso ParsearTaXml(string xml)
        {
            XmlDocument taDoc = new XmlDocument();
            taDoc.LoadXml(xml);

            string token = taDoc.SelectSingleNode("//token")?.InnerText ?? throw new Exception("Token no encontrado en TA");
            string sign = taDoc.SelectSingleNode("//sign")?.InnerText ?? throw new Exception("Sign no encontrado en TA");
            string expirationText = taDoc.SelectSingleNode("//expirationTime")?.InnerText ?? throw new Exception("ExpirationTime no encontrado en TA");
            string generationText = taDoc.SelectSingleNode("//generationTime")?.InnerText ?? throw new Exception("GenerationTime no encontrado en TA");

            return new AfipTicketAcceso
            {
                Token = token,
                Sign = sign,
                ExpirationTime = DateTime.Parse(expirationText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                GenerationTime = DateTime.Parse(generationText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            };
        }

        public static byte[] FirmaBytesMensaje(byte[] argBytesMsg, X509Certificate2 argCertFirmante)
        {
            try
            {
                ContentInfo infoContenido = new ContentInfo(argBytesMsg);
                SignedCms cmsFirmado = new SignedCms(infoContenido);
                CmsSigner cmsFirmante = new CmsSigner(argCertFirmante);
                cmsFirmante.IncludeOption = X509IncludeOption.EndCertOnly;
                cmsFirmado.ComputeSignature(cmsFirmante);
                return cmsFirmado.Encode();
            }
            catch (Exception excepcionAlFirmar)
            {
                throw new Exception("Error al firmar: " + excepcionAlFirmar.Message, excepcionAlFirmar);
            }
        }

        private X509Certificate2 LoadCertificate(AfipConfig config)
        {
            // Prioridad 1: Certificados desde la DB (bytes)
            if (config.CrtBytes != null && config.CrtBytes.Length > 0 &&
                config.KeyBytes != null && config.KeyBytes.Length > 0)
            {
                _logger.LogInformation("Cargando certificado desde base de datos. CRT: {CrtSize} bytes, KEY: {KeySize} bytes",
                    config.CrtBytes.Length, config.KeyBytes.Length);
                var certPem = Encoding.UTF8.GetString(config.CrtBytes).Trim().Trim('\uFEFF');
                var keyPem = Encoding.UTF8.GetString(config.KeyBytes).Trim().Trim('\uFEFF');
                _logger.LogInformation("CRT empieza con: {CrtStart}", certPem.Substring(0, Math.Min(40, certPem.Length)));
                _logger.LogInformation("KEY empieza con: {KeyStart}", keyPem.Substring(0, Math.Min(40, keyPem.Length)));
                return LoadCertificateFromPemStrings(certPem, keyPem);
            }

            // Prioridad 2: Archivos CRT + KEY
            if (!string.IsNullOrEmpty(config.CrtPath) && !string.IsNullOrEmpty(config.KeyPath))
            {
                _logger.LogInformation("Cargando certificado desde CRT + KEY archivos");
                var certPem = File.ReadAllText(config.CrtPath);
                var keyPem = File.ReadAllText(config.KeyPath);
                return LoadCertificateFromPemStrings(certPem, keyPem);
            }

            // Prioridad 3: PFX
            if (!string.IsNullOrEmpty(config.PfxPath))
            {
                _logger.LogInformation("Cargando certificado desde PFX");
                return LoadCertificateFromPfx(config.PfxPath, config.PfxPassword);
            }

            throw new Exception("No se configuró ningún certificado");
        }

        private static X509Certificate2 LoadCertificateFromPemBytes(byte[] crtBytes, byte[] keyBytes)
        {
            var certPem = Encoding.UTF8.GetString(crtBytes).Trim().Trim('\uFEFF');
            var keyPem = Encoding.UTF8.GetString(keyBytes).Trim().Trim('\uFEFF');
            return LoadCertificateFromPemStrings(certPem, keyPem);
        }

        private static X509Certificate2 LoadCertificateFromPemStrings(string certPem, string keyPem)
        {
            certPem = certPem.Trim().Replace("\r\n", "\n");
            keyPem = keyPem.Trim().Replace("\r\n", "\n");
            var cert = X509Certificate2.CreateFromPem(certPem, keyPem);
            var exported = cert.Export(X509ContentType.Pfx);
            var finalCert = new X509Certificate2(exported, (string?)null, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);

            if (!finalCert.HasPrivateKey)
            {
                throw new Exception("El certificado CRT+KEY no contiene la clave privada");
            }

            return finalCert;
        }

        public static X509Certificate2 LoadCertificateFromPfx(string rutaPfx, string password)
        {
            var cert = new X509Certificate2(rutaPfx, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);

            if (!cert.HasPrivateKey)
            {
                throw new Exception("El certificado PFX no contiene la clave privada");
            }

            return cert;
        }

        private static int _globalUniqueID = 0;

        private static DateTime GetBuenosAiresTime()
        {
            var buenosAiresZone = TimeZoneInfo.FindSystemTimeZoneById("America/Buenos_Aires");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, buenosAiresZone);
        }

        private string GenerateLoginTicketRequest(AfipConfig config)
        {
            string xmlTemplate = $@"<loginTicketRequest><header>    `<uniqueId></uniqueId><generationTime></generationTime><expirationTime></expirationTime></header><service></service></loginTicketRequest>";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlTemplate);

            _globalUniqueID += 1;

            var xmlNodoUniqueId = xmlDoc.SelectSingleNode("//uniqueId") ?? throw new Exception("No se encontró nodo uniqueId");
            var xmlNodoGenerationTime = xmlDoc.SelectSingleNode("//generationTime") ?? throw new Exception("No se encontró nodo generationTime");
            var xmlNodoExpirationTime = xmlDoc.SelectSingleNode("//expirationTime") ?? throw new Exception("No se encontró nodo expirationTime");
            var xmlNodoService = xmlDoc.SelectSingleNode("//service") ?? throw new Exception("No se encontró nodo service");

            var buenosAiresNow = GetBuenosAiresTime();
            xmlNodoGenerationTime.InnerText = buenosAiresNow.AddMinutes(-10).ToString("s");
            xmlNodoExpirationTime.InnerText = buenosAiresNow.AddMinutes(+10).ToString("s");

            xmlNodoUniqueId.InnerText = Convert.ToString(_globalUniqueID);
            xmlNodoService.InnerText = "wsfe";

            var xml = xmlDoc.OuterXml;
            _logger.LogInformation("XML generado: {Xml}", xml);
            return xml;
        }

        private async Task<(AfipTicketAcceso ticket, string credentialsXml)> SendToWSAAAsync(string signedRequestBase64, AfipConfig config)
        {
            try
            {
                using var client = new HttpClient();

                var soapRequest = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:wsaa=""http://wsaa.view.sua.dvadac.desein.afip.gov"">
<soapenv:Header/>
<soapenv:Body>
    <wsaa:loginCms>
        <wsaa:in0>{System.Security.SecurityElement.Escape(signedRequestBase64)}</wsaa:in0>
    </wsaa:loginCms>
</soapenv:Body>
</soapenv:Envelope>";

                _logger.LogInformation("Enviando request a WSAA: {Url}", config.WsaaUrl);

                var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "");

                var response = await client.PostAsync(config.WsaaUrl, content);
                var responseXml = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Respuesta de WSAA (StatusCode: {StatusCode})", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error en respuesta WSAA: {StatusCode} - {Response}", response.StatusCode, responseXml);
                    throw new Exception($"Error en WSAA: {response.StatusCode} - {responseXml}");
                }

                _logger.LogInformation("Respuesta exitosa de WSAA");
                return ParseWSAAResponse(responseXml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar request a WSAA");
                throw;
            }
        }

        private (AfipTicketAcceso ticket, string credentialsXml) ParseWSAAResponse(string responseXml)
        {
            try
            {
                _logger.LogInformation("Parseando respuesta WSAA");

                var doc = new XmlDocument();
                doc.LoadXml(responseXml);
                var taNode = doc.GetElementsByTagName("loginCmsReturn")[0];

                if (taNode == null || string.IsNullOrEmpty(taNode.InnerText))
                {
                    _logger.LogError("No se encontró loginCmsReturn en la respuesta. XML: {Xml}", responseXml);
                    throw new Exception("No se pudo obtener el TA");
                }

                var credentialsXml = taNode.InnerText;
                _logger.LogInformation("TA XML extraído exitosamente");

                var ticket = ParsearTaXml(credentialsXml);
                _logger.LogInformation("Ticket de acceso parseado exitosamente. Expira: {ExpirationTime}", ticket.ExpirationTime);

                return (ticket, credentialsXml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al parsear respuesta WSAA");
                throw new Exception("Error al parsear respuesta de WSAA", ex);
            }
        }
    }
}
