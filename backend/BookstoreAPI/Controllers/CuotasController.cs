using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using BookstoreAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuotasController : ControllerBase
    {
        private readonly ICuotaRepository _cuotaRepository;
        private readonly ILogger<CuotasController> _logger;

        public CuotasController(ICuotaRepository cuotaRepository, ILogger<CuotasController> logger)
        {
            _cuotaRepository = cuotaRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCuotas([FromQuery] int? zonaId, [FromQuery] DateTime? fechaCorte, [FromQuery] int? vendedorId, [FromQuery] string? comprobante, [FromQuery] int? clienteId)
        {
            try
            {
                var cuotas = await _cuotaRepository.GetCuotasByFiltrosAsync(zonaId, fechaCorte, vendedorId, comprobante, clienteId);
                return Ok(cuotas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuotas");
                return StatusCode(500, new { message = "Error al obtener cuotas", error = ex.Message });
            }
        }

        [HttpPost("{cuotaId}/pagos")]
        public async Task<IActionResult> CreatePago(int cuotaId, [FromBody] CreatePagoCuotaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var pago = new PagoCuota
                {
                    NroReferencia = dto.NroReferencia,
                    Fecha = dto.Fecha,
                    Importe = dto.Importe
                };

                var created = await _cuotaRepository.CreatePagoAsync(cuotaId, pago);
                return Ok(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago de cuota {CuotaId}", cuotaId);
                return StatusCode(500, new { message = "Error al registrar pago", error = ex.Message });
            }
        }

        [HttpPost("comprobante/{comprobanteId}/pago")]
        public async Task<IActionResult> CreatePagoComprobante(int comprobanteId, [FromBody] CreatePagoComprobanteDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (dto.Importe <= 0)
                    return BadRequest(new { message = "El importe debe ser mayor a 0" });

                await _cuotaRepository.CreatePagoComprobanteAsync(comprobanteId, dto.NroReferencia, dto.Importe, dto.Fecha);
                return Ok(new { message = "Pago imputado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago de comprobante {ComprobanteId}", comprobanteId);
                return StatusCode(500, new { message = "Error al registrar pago", error = ex.Message });
            }
        }

        [HttpDelete("pagos/{pagoId}")]
        public async Task<IActionResult> DeletePago(int pagoId)
        {
            try
            {
                var deleted = await _cuotaRepository.DeletePagoAsync(pagoId);
                if (!deleted)
                    return NotFound(new { message = $"Pago con ID {pagoId} no encontrado" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pago de cuota");
                return StatusCode(500, new { message = "Error al eliminar pago", error = ex.Message });
            }
        }
    }
}
