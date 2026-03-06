using BookstoreAPI.DTOs;
using BookstoreAPI.Repositories;
using BookstoreAPI.Services.Afip;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracionController : ControllerBase
    {
        private readonly IConfiguracionRepository _repo;
        private readonly IAfipFacturacionService _afipFacturacion;
        private readonly ILogger<ConfiguracionController> _logger;

        public ConfiguracionController(
            IConfiguracionRepository repo,
            IAfipFacturacionService afipFacturacion,
            ILogger<ConfiguracionController> logger)
        {
            _repo = repo;
            _afipFacturacion = afipFacturacion;
            _logger = logger;
        }

        [HttpGet("afip")]
        public async Task<IActionResult> GetAfipConfig()
        {
            try
            {
                var valores = await _repo.GetAllAsync();
                var crt = await _repo.GetBinaryValueAsync("Afip_Crt");
                var key = await _repo.GetBinaryValueAsync("Afip_Key");

                var dto = new AfipConfigDto
                {
                    CUIT = valores.GetValueOrDefault("Afip_CUIT", ""),
                    WsaaUrl = valores.GetValueOrDefault("Afip_WsaaUrl", ""),
                    WsfevUrl = valores.GetValueOrDefault("Afip_WsfevUrl", ""),
                    PuntoVenta = int.TryParse(valores.GetValueOrDefault("Afip_PuntoVenta", "0"), out var pv) ? pv : 0,
                    IsProduction = valores.GetValueOrDefault("Afip_IsProduction", "false").Equals("true", StringComparison.OrdinalIgnoreCase),
                    TieneCrt = crt != null && crt.Length > 0,
                    TieneKey = key != null && key.Length > 0
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración AFIP");
                return StatusCode(500, new { message = "Error al obtener configuración", error = ex.Message });
            }
        }

        [HttpPut("afip")]
        public async Task<IActionResult> UpdateAfipConfig([FromBody] AfipConfigUpdateDto dto)
        {
            try
            {
                var valores = new Dictionary<string, string>
                {
                    ["Afip_CUIT"] = dto.CUIT,
                    ["Afip_WsaaUrl"] = dto.WsaaUrl,
                    ["Afip_WsfevUrl"] = dto.WsfevUrl,
                    ["Afip_PuntoVenta"] = dto.PuntoVenta.ToString(),
                    ["Afip_IsProduction"] = dto.IsProduction.ToString().ToLower()
                };

                await _repo.SaveAllAsync(valores);

                // Guardar certificado CRT si viene
                if (!string.IsNullOrEmpty(dto.CrtBase64))
                {
                    var crtBytes = Convert.FromBase64String(dto.CrtBase64);
                    await _repo.SetBinaryValueAsync("Afip_Crt", crtBytes);
                }

                // Guardar clave KEY si viene
                if (!string.IsNullOrEmpty(dto.KeyBase64))
                {
                    var keyBytes = Convert.FromBase64String(dto.KeyBase64);
                    await _repo.SetBinaryValueAsync("Afip_Key", keyBytes);
                }

                return Ok(new { message = "Configuración guardada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar configuración AFIP");
                return StatusCode(500, new { message = "Error al guardar configuración", error = ex.Message });
            }
        }

        [HttpGet("afip/diagnostico-cert")]
        public async Task<IActionResult> DiagnosticoCert()
        {
            var crt = await _repo.GetBinaryValueAsync("Afip_Crt");
            var key = await _repo.GetBinaryValueAsync("Afip_Key");

            var crtText = crt != null ? Encoding.UTF8.GetString(crt) : null;
            var keyText = key != null ? Encoding.UTF8.GetString(key) : null;

            return Ok(new
            {
                crtSize = crt?.Length,
                keySize = key?.Length,
                crtPrimeros100 = crtText?.Substring(0, Math.Min(100, crtText.Length)),
                keyPrimeros100 = keyText?.Substring(0, Math.Min(100, keyText.Length)),
                crtUltimos50 = crtText != null && crtText.Length > 50 ? crtText.Substring(crtText.Length - 50) : crtText,
                keyUltimos50 = keyText != null && keyText.Length > 50 ? keyText.Substring(keyText.Length - 50) : keyText
            });
        }

        [HttpGet("afip/ultimo-comprobante")]
        public async Task<IActionResult> GetUltimoComprobante([FromQuery] int? puntoVenta)
        {
            try
            {
                var valores = await _repo.GetAllAsync();
                var pv = puntoVenta ?? (int.TryParse(valores.GetValueOrDefault("Afip_PuntoVenta", "0"), out var p) ? p : 0);

                var tiposComprobante = new Dictionary<int, string>
                {
                    { 1, "Factura A" },
                    { 6, "Factura B" },
                    { 11, "Factura C" },
                    { 3, "Nota de Crédito A" },
                    { 8, "Nota de Crédito B" },
                    { 13, "Nota de Crédito C" }
                };

                var resultados = new List<UltimoComprobanteDto>();

                foreach (var tipo in tiposComprobante)
                {
                    var ultimo = await _afipFacturacion.GetUltimoComprobanteAutorizadoAsync(pv, tipo.Key);
                    resultados.Add(new UltimoComprobanteDto
                    {
                        PuntoVenta = pv,
                        TipoComprobante = tipo.Key,
                        TipoComprobanteDescripcion = tipo.Value,
                        UltimoNumero = ultimo
                    });
                }

                return Ok(resultados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar último comprobante");
                return StatusCode(500, new { message = "Error al consultar", error = ex.Message });
            }
        }
    }
}
