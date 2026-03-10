using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using BookstoreAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CuotasProveedoresController : ControllerBase
    {
        private readonly ICuotaProveedorRepository _repository;
        private readonly ILogger<CuotasProveedoresController> _logger;

        public CuotasProveedoresController(ICuotaProveedorRepository repository, ILogger<CuotasProveedoresController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCuotas(
            [FromQuery] int? proveedorId,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] string? estado)
        {
            try
            {
                var cuotas = await _repository.GetCuotasByFiltrosAsync(proveedorId, fechaDesde, fechaHasta, estado);
                return Ok(cuotas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuotas de proveedores");
                return StatusCode(500, new { message = "Error al obtener cuotas de proveedores", error = ex.Message });
            }
        }

        [HttpPost("{cuotaId}/pagos")]
        public async Task<IActionResult> CreatePago(int cuotaId, [FromBody] CreatePagoCuotaProveedorDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var pago = new PagoCuotaProveedor
                {
                    NroReferencia = dto.NroReferencia,
                    Fecha = dto.Fecha,
                    Importe = dto.Importe
                };

                var created = await _repository.CreatePagoAsync(cuotaId, pago);
                return Ok(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago de cuota de proveedor");
                return StatusCode(500, new { message = "Error al registrar pago", error = ex.Message });
            }
        }

        [HttpDelete("pagos/{pagoId}")]
        public async Task<IActionResult> DeletePago(int pagoId)
        {
            try
            {
                var deleted = await _repository.DeletePagoAsync(pagoId);
                if (!deleted)
                    return NotFound(new { message = $"Pago con ID {pagoId} no encontrado" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pago de cuota de proveedor");
                return StatusCode(500, new { message = "Error al eliminar pago", error = ex.Message });
            }
        }
    }
}
