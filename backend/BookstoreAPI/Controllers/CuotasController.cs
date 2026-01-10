using BookstoreAPI.DTOs;
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
        public async Task<IActionResult> GetCuotas([FromQuery] int? zonaId, [FromQuery] int? mes, [FromQuery] int? anio)
        {
            try
            {
                var cuotas = await _cuotaRepository.GetCuotasByFiltrosAsync(zonaId, mes, anio);
                return Ok(cuotas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuotas");
                return StatusCode(500, new { message = "Error al obtener cuotas", error = ex.Message });
            }
        }

        [HttpPut("{id}/importe-pagado")]
        public async Task<IActionResult> UpdateImportePagado(int id, [FromBody] UpdateImportePagadoDto dto)
        {
            try
            {
                // Todas las cuotas (incluyendo cuota 0/contraentrega) están en la tabla cuotas
                var updated = await _cuotaRepository.UpdateImportePagadoAsync(id, dto.ImportePagado);

                if (!updated)
                    return NotFound(new { message = $"Cuota con ID {id} no encontrada" });

                return Ok(new { message = "Importe pagado actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar importe pagado de cuota {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar importe pagado", error = ex.Message });
            }
        }
    }
}
