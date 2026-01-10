using Afip.WsfeV1;
using BookstoreAPI.Models;
using BookstoreAPI.Models.Afip;
using BookstoreAPI.Repositories;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using System.Xml;

namespace BookstoreAPI.Services.Afip
{
    public class AfipFacturacionService : IAfipFacturacionService
    {
        private readonly AfipConfig _config;
        private readonly IAfipAuthService _authService;
        private readonly IClienteRepository _clienteRepository;
        private readonly ILogger<AfipFacturacionService> _logger;

        public AfipFacturacionService(
            IOptions<AfipConfig> config,
            IAfipAuthService authService,
            IClienteRepository clienteRepository,
            ILogger<AfipFacturacionService> logger)
        {
            _config = config.Value;
            _authService = authService;
            _clienteRepository = clienteRepository;
            _logger = logger;
        }

        public async Task<AfipCAEResponse> SolicitarCAEAsync(Comprobante comprobante, List<ComprobanteDetalle> detalles)
        {
            // Por defecto usa Factura C (tipo 11) sin comprobante asociado
            return await SolicitarCAEAsync(comprobante, detalles, 11, null);
        }

        public async Task<AfipCAEResponse> SolicitarCAEAsync(Comprobante comprobante, List<ComprobanteDetalle> detalles, int tipoComprobanteAfip)
        {
            // Sin comprobante asociado
            return await SolicitarCAEAsync(comprobante, detalles, tipoComprobanteAfip, null);
        }

        public async Task<AfipCAEResponse> SolicitarCAEAsync(Comprobante comprobante, List<ComprobanteDetalle> detalles, int tipoComprobanteAfip, ComprobanteAsociadoInfo? comprobanteAsociado)
        {
            try
            {
                _logger.LogInformation("Iniciando solicitud de CAE para comprobante tipo {TipoComprobante}", tipoComprobanteAfip);

                // Obtener ticket de acceso
                var ticket = await _authService.GetTicketAccesoAsync();

                // Obtener datos del cliente
                var cliente = await _clienteRepository.GetByIdAsync(comprobante.Cliente_Id);
                if (cliente == null)
                {
                    throw new Exception($"Cliente {comprobante.Cliente_Id} no encontrado");
                }

                // Usar el tipo de comprobante pasado como parámetro
                int tipoComprobante = tipoComprobanteAfip;
                int tipoDocumento = DeterminarTipoDocumento(cliente.TipoDocumento);

                // Obtener último comprobante autorizado
                int ultimoComprobante = await GetUltimoComprobanteAutorizadoAsync(_config.PuntoVenta, tipoComprobante);
                int numeroComprobante = ultimoComprobante + 1;

                // Calcular totales
                decimal subtotal = detalles.Sum(d => d.Subtotal);
                decimal iva = CalcularIVA(subtotal, cliente.CategoriaIva);
                decimal total = subtotal + iva;

                // Crear SOAP request
                var fechaFormato = comprobante.Fecha.ToString("yyyyMMdd");
                var concepto = 1; // 1=Productos, 2=Servicios, 3=Productos y Servicios

                /*               var soapRequest = $@"
               <soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
                   <soap:Header/>
                   <soap:Body>
                       <ar:FECAESolicitar>
                           <ar:Auth>
                               <ar:Token>{ticket.Token}</ar:Token>
                               <ar:Sign>{ticket.Sign}</ar:Sign>
                               <ar:Cuit>{_config.CUIT}</ar:Cuit>
                           </ar:Auth>
                           <ar:FeCAEReq>
                               <ar:FeCabReq>
                                   <ar:CantReg>1</ar:CantReg>
                                   <ar:PtoVta>{_config.PuntoVenta}</ar:PtoVta>
                                   <ar:CbteTipo>{tipoComprobante}</ar:CbteTipo>
                               </ar:FeCabReq>
                               <ar:FeDetReq>
                                   <ar:FECAEDetRequest>
                                       <ar:Concepto>{concepto}</ar:Concepto>
                                       <ar:DocTipo>{tipoDocumento}</ar:DocTipo>
                                       <ar:DocNro>{long.Parse(cliente.NroDocumento?.Replace("-", "").Replace(".", "") ?? "0")}</ar:DocNro>
                                       <ar:CbteDesde>{numeroComprobante}</ar:CbteDesde>
                                       <ar:CbteHasta>{numeroComprobante}</ar:CbteHasta>
                                       <ar:CbteFch>{fechaFormato}</ar:CbteFch>
                                       <ar:ImpTotal>{total.ToString("F2")}</ar:ImpTotal>
                                       <ar:ImpTotConc>0</ar:ImpTotConc>
                                       <ar:ImpNeto>{total.ToString("F2")}</ar:ImpNeto>
                                       <ar:ImpOpEx>0</ar:ImpOpEx>
                                       <ar:ImpIVA>0</ar:ImpIVA>
                                       <ar:MonId>PES</ar:MonId>
                                       <ar:MonCotiz>1</ar:MonCotiz>
                                       <ar:CondicionIVAReceptorId>5</ar:CondicionIVAReceptorId>
                                   </ar:FECAEDetRequest>
                               </ar:FeDetReq>
                           </ar:FeCAEReq>
                       </ar:FECAESolicitar>
                   </soap:Body>
               </soap:Envelope>";
               */


                // Crear el detalle del request
                var detRequest = new FECAEDetRequest
                {
                    Concepto = 1,
                    DocTipo = tipoDocumento,
                    DocNro = cliente.NroDocumento,
                    CbteDesde = numeroComprobante,
                    CbteHasta = numeroComprobante,
                    CbteFch = DateTime.Now.ToString("yyyyMMdd"),
                    ImpTotal = total.ToString("F2", CultureInfo.InvariantCulture),
                    ImpTotConc = 0m,
                    ImpNeto = total.ToString("F2", CultureInfo.InvariantCulture),
                    ImpOpEx = 0m,
                    ImpTrib = 0m,
                    ImpIVA = 0m,
                    MonId = "PES",
                    MonCotiz = 1m,
                    CondicionIVAReceptorId = 5
                };

                // Agregar comprobante asociado si existe (para Notas de Crédito/Débito)
                if (comprobanteAsociado != null)
                {
                    _logger.LogInformation("Agregando comprobante asociado: Tipo={Tipo}, PtoVta={PtoVta}, Nro={Nro}",
                        comprobanteAsociado.Tipo, comprobanteAsociado.PuntoVenta, comprobanteAsociado.Numero);

                    detRequest.CbtesAsoc = new List<CbteAsoc>
                    {
                        new CbteAsoc
                        {
                            Tipo = comprobanteAsociado.Tipo,
                            PtoVta = comprobanteAsociado.PuntoVenta,
                            Nro = comprobanteAsociado.Numero,
                            CbteFch = comprobanteAsociado.Fecha.ToString("yyyyMMdd")
                        }
                    };
                }

                var env = new Envelope
                {
                    Body = new Body
                    {
                        FECAESolicitar = new FECAESolicitar
                        {
                            Auth = new Auth { Token = ticket.Token ,
                                Sign = ticket.Sign, Cuit = _config.CUIT },
                            FeCAEReq = new FeCAEReq
                            {
                                FeCabReq = new FeCabReq { CantReg = 1, PtoVta = _config.PuntoVenta, CbteTipo = tipoComprobante },
                                FeDetReq = new FeDetReq
                                {
                                    FECAEDetRequest = new List<FECAEDetRequest> { detRequest }
                                }
                            }
                        }
                    }
                };

                string soapRequest = SoapXmlHelper.Serialize(env);
                // Enviar solicitud
                using var client = new HttpClient();
                var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "http://ar.gov.afip.dif.FEV1/FECAESolicitar");

                var response = await client.PostAsync(_config.WsfevUrl, content);
                var responseXml = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("Respuesta WSFEv1: {Response}", responseXml);

                // Procesar respuesta
                var result = ParsearRespuestaCAE(responseXml, numeroComprobante);

                if (result.Success)
                {
                    _logger.LogInformation("CAE obtenido exitosamente: {CAE}", result.CAE);
                }
                else
                {
                    _logger.LogWarning("Error al obtener CAE: {Errores}", string.Join(", ", result.Errores));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al solicitar CAE");
                return new AfipCAEResponse
                {
                    Success = false,
                    Errores = new List<string> { ex.Message }
                };
            }
        }

        private AfipCAEResponse ParsearRespuestaCAE(string responseXml, int numeroComprobante)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responseXml);

            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            nsManager.AddNamespace("ns1", "http://ar.gov.afip.dif.FEV1/");

            var result = new AfipCAEResponse
            {
                NumeroComprobante = $"{_config.PuntoVenta:D5}-{numeroComprobante:D8}"
            };

            // Verificar errores
            var erroresNodes = xmlDoc.SelectNodes("//ns1:Err", nsManager);
            if (erroresNodes != null)
            {
                foreach (XmlNode errorNode in erroresNodes)
                {
                    var codeNode = errorNode.SelectSingleNode("ns1:Code", nsManager);
                    var msgNode = errorNode.SelectSingleNode("ns1:Msg", nsManager);
                    if (codeNode != null && msgNode != null)
                    {
                        result.Errores.Add($"[{codeNode.InnerText}] {msgNode.InnerText}");
                    }
                }
            }

            // Verificar observaciones
            var obsNodes = xmlDoc.SelectNodes("//ns1:Obs", nsManager);
            if (obsNodes != null)
            {
                foreach (XmlNode obsNode in obsNodes)
                {
                    var codeNode = obsNode.SelectSingleNode("ns1:Code", nsManager);
                    var msgNode = obsNode.SelectSingleNode("ns1:Msg", nsManager);
                    if (codeNode != null && msgNode != null)
                    {
                        result.Observaciones.Add($"[{codeNode.InnerText}] {msgNode.InnerText}");
                    }
                }
            }

            // Obtener CAE y fecha de vencimiento
            var caeNode = xmlDoc.SelectSingleNode("//ns1:CAE", nsManager);
            var vtoNode = xmlDoc.SelectSingleNode("//ns1:CAEFchVto", nsManager);
            var resultadoNode = xmlDoc.SelectSingleNode("//ns1:Resultado", nsManager);

            if (caeNode != null && vtoNode != null && resultadoNode?.InnerText == "A")
            {
                result.Success = true;
                result.CAE = caeNode.InnerText;

                if (DateTime.TryParseExact(vtoNode.InnerText, "yyyyMMdd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime vtoDate))
                {
                    result.CAEVencimiento = vtoDate;
                }
            }
            else
            {
                result.Success = false;
                if (string.IsNullOrEmpty(result.Errores.FirstOrDefault()))
                {
                    result.Errores.Add($"Comprobante rechazado. Resultado: {resultadoNode?.InnerText}");
                }
            }

            return result;
        }

        public async Task<int> GetUltimoComprobanteAutorizadoAsync(int puntoVenta, int tipoComprobante)
        {
            try
            {
                var ticket = await _authService.GetTicketAccesoAsync();

                var soapRequest = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
    <soapenv:Header/>
    <soapenv:Body>
        <ar:FECompUltimoAutorizado>
            <ar:Auth>
                <ar:Token>{ticket.Token}</ar:Token>
                <ar:Sign>{ticket.Sign}</ar:Sign>
                <ar:Cuit>{_config.CUIT}</ar:Cuit>
            </ar:Auth>
            <ar:PtoVta>{puntoVenta}</ar:PtoVta>
            <ar:CbteTipo>{tipoComprobante}</ar:CbteTipo>
        </ar:FECompUltimoAutorizado>
    </soapenv:Body>
</soapenv:Envelope>";

                using var client = new HttpClient();
                var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "http://ar.gov.afip.dif.FEV1/FECompUltimoAutorizado");

                var response = await client.PostAsync(_config.WsfevUrl, content);
                var responseXml = await response.Content.ReadAsStringAsync();

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseXml);

                var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                nsManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                nsManager.AddNamespace("ns1", "http://ar.gov.afip.dif.FEV1/");

                var cbteNroNode = xmlDoc.SelectSingleNode("//ns1:CbteNro", nsManager);
                if (cbteNroNode != null && int.TryParse(cbteNroNode.InnerText, out int cbteNro))
                {
                    return cbteNro;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener último comprobante autorizado");
                return 0;
            }
        }


        private int DeterminarTipoComprobante(string? categoriaIva)
        {
            // Factura A = 1, Factura B = 6, Factura C = 11
            return categoriaIva?.ToUpper() switch
            {
                "RESPONSABLE INSCRIPTO" => 1, // Factura A
                "MONOTRIBUTO" => 6,           // Factura B
                "CONSUMIDOR FINAL" => 11,       // Factura C
                "EXENTO" => 6,                 // Factura B
                _ => 6                         // Factura B por defecto
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

        private decimal CalcularIVA(decimal subtotal, string? categoriaIva)
        {
            // Solo calcular IVA para Responsables Inscriptos
            if (categoriaIva?.ToUpper() == "RESPONSABLE INSCRIPTO")
            {
                return subtotal * 0.21m; // IVA 21%
            }

            return 0;
        }
    }
}
