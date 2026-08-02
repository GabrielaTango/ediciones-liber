using BookstoreAPI.DTOs;
using BookstoreAPI.Repositories;
using BookstoreAPI.Services.Afip;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConfiguracionController : ControllerBase
    {
        private readonly IConfiguracionRepository _repo;
        private readonly IAfipFacturacionService _afipFacturacion;
        private readonly IAfipCertificadoService _afipCertificado;
        private readonly ILogger<ConfiguracionController> _logger;
        private readonly IConfiguration _configuration;

        public ConfiguracionController(
            IConfiguracionRepository repo,
            IAfipFacturacionService afipFacturacion,
            IAfipCertificadoService afipCertificado,
            ILogger<ConfiguracionController> logger,
            IConfiguration configuration)
        {
            _repo = repo;
            _afipFacturacion = afipFacturacion;
            _afipCertificado = afipCertificado;
            _logger = logger;
            _configuration = configuration;
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

        /// <summary>
        /// Estado del certificado AFIP (vigencia y días restantes). Siempre responde 200:
        /// un problema con el certificado no debe romper el dashboard.
        /// </summary>
        [HttpGet("afip/certificado")]
        public async Task<IActionResult> GetEstadoCertificado()
        {
            var estado = await _afipCertificado.GetEstadoAsync();
            return Ok(estado);
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

        // ===== BACKUP / RESTORE =====

        private const string DefaultBackupPath = "/backup";

        [HttpGet("backup-path")]
        public async Task<IActionResult> GetBackupPath()
        {
            var ruta = await _repo.GetValueAsync("Backup_RutaCarpeta");
            return Ok(new { ruta = ruta ?? DefaultBackupPath });
        }

        [HttpPut("backup-path")]
        public async Task<IActionResult> SetBackupPath([FromBody] BackupPathDto dto)
        {
            await _repo.SetValueAsync("Backup_RutaCarpeta", dto.Ruta);
            return Ok(new { message = "Ruta guardada correctamente" });
        }

        [HttpGet("backup/archivos")]
        public async Task<IActionResult> ListarArchivosBackup()
        {
            var ruta = await _repo.GetValueAsync("Backup_RutaCarpeta");
            if (string.IsNullOrEmpty(ruta))
                ruta = DefaultBackupPath;
            if (!Directory.Exists(ruta))
                return Ok(Array.Empty<string>());

            var archivos = Directory.GetFiles(ruta, "*.sql")
                .Select(Path.GetFileName)
                .OrderByDescending(f => f)
                .ToArray();

            return Ok(archivos);
        }

        [HttpPost("backup")]
        public async Task<IActionResult> RealizarBackup()
        {
            try
            {
                var ruta = await _repo.GetValueAsync("Backup_RutaCarpeta");
                if (string.IsNullOrEmpty(ruta))
                    ruta = DefaultBackupPath;

                if (!Directory.Exists(ruta))
                    Directory.CreateDirectory(ruta);

                var connStr = _configuration.GetConnectionString("DefaultConnection")!;
                var builder = new MySqlConnectionStringBuilder(connStr);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"backup_{builder.Database}_{timestamp}.sql";
                var filePath = Path.Combine(ruta, fileName);

                var args = $"--host={builder.Server} --user={builder.UserID} --password={builder.Password} --port={builder.Port} --single-transaction --routines --triggers {builder.Database}";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "mysqldump",
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("mysqldump error: {Error}", error);
                    return StatusCode(500, new { message = "Error al realizar backup", error });
                }

                await System.IO.File.WriteAllTextAsync(filePath, output, Encoding.UTF8);

                return Ok(new { message = "Backup realizado correctamente", archivo = fileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar backup");
                return StatusCode(500, new { message = "Error al realizar backup", error = ex.Message });
            }
        }

        [HttpPost("restore")]
        public async Task<IActionResult> RestaurarBackup([FromBody] RestoreDto dto)
        {
            try
            {
                var ruta = await _repo.GetValueAsync("Backup_RutaCarpeta");
                if (string.IsNullOrEmpty(ruta))
                    ruta = DefaultBackupPath;

                var filePath = Path.Combine(ruta, dto.Archivo);
                if (!System.IO.File.Exists(filePath))
                    return BadRequest(new { message = "El archivo de backup no existe" });

                var connStr = _configuration.GetConnectionString("DefaultConnection")!;
                var builder = new MySqlConnectionStringBuilder(connStr);

                var args = $"--host={builder.Server} --user={builder.UserID} --password={builder.Password} --port={builder.Port} {builder.Database}";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "mysql",
                        Arguments = args,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var sqlContent = await System.IO.File.ReadAllTextAsync(filePath, Encoding.UTF8);
                await process.StandardInput.WriteAsync(sqlContent);
                process.StandardInput.Close();

                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("mysql restore error: {Error}", error);
                    return StatusCode(500, new { message = "Error al restaurar backup", error });
                }

                return Ok(new { message = "Backup restaurado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restaurar backup");
                return StatusCode(500, new { message = "Error al restaurar backup", error = ex.Message });
            }
        }
        [HttpPost("vaciar-datos")]
        public async Task<IActionResult> VaciarDatos()
        {
            try
            {
                using var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync();

                var tablasOrdenadas = new[]
                {
                    "pagos_cuotas_proveedores",
                    "pagos_cuotas",
                    "cuotas_proveedores",
                    "comprobantes_proveedores",
                    "cuotas",
                    "comprobante_detalle",
                    "comprobantes",
                    "remitos",
                    "precios",
                    "articulos",
                    "clientes",
                    "vendedores",
                    "subzonas",
                    "transportes",
                    "zonas",
                    "provincias",
                    "tipodocumento",
                    "condicionVenta",
                    "listas",
                    "categorias_gasto",
                    "gastos",
                    "customers"
                };

                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    await connection.ExecuteAsync("SET FOREIGN_KEY_CHECKS = 0", transaction: transaction);

                    foreach (var tabla in tablasOrdenadas)
                    {
                        await connection.ExecuteAsync($"TRUNCATE TABLE `{tabla}`", transaction: transaction);
                    }

                    await connection.ExecuteAsync("SET FOREIGN_KEY_CHECKS = 1", transaction: transaction);

                    await transaction.CommitAsync();

                    return Ok(new { message = "Base de datos vaciada correctamente. La configuración se mantuvo." });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al vaciar la base de datos");
                return StatusCode(500, new { message = "Error al vaciar la base de datos", error = ex.Message });
            }
        }
    }
}
