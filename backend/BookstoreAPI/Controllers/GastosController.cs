using Microsoft.AspNetCore.Mvc;
using BookstoreAPI.DTOs;
using BookstoreAPI.Services;
using BookstoreAPI.Services.Pdf;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GastosController : ControllerBase
    {
        private readonly IGastoService _gastoService;
        private readonly IGastoPdfService _gastoPdfService;
        private readonly ILogger<GastosController> _logger;

        public GastosController(IGastoService gastoService, IGastoPdfService gastoPdfService, ILogger<GastosController> logger)
        {
            _gastoService = gastoService;
            _gastoPdfService = gastoPdfService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var gastos = await _gastoService.GetAllGastosAsync();
                return Ok(gastos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los gastos");
                return StatusCode(500, new { message = "Error al obtener los gastos" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var gasto = await _gastoService.GetGastoByIdAsync(id);
                if (gasto == null)
                {
                    return NotFound(new { message = $"No se encontró el gasto con ID {id}" });
                }
                return Ok(gasto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el gasto con ID {Id}", id);
                return StatusCode(500, new { message = "Error al obtener el gasto" });
            }
        }

        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            try
            {
                var categorias = await _gastoService.GetCategoriasAsync();
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las categorías");
                return StatusCode(500, new { message = "Error al obtener las categorías" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGastoDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var gasto = await _gastoService.CreateGastoAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = gasto.Id }, gasto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el gasto");
                return StatusCode(500, new { message = "Error al crear el gasto" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGastoDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var gasto = await _gastoService.UpdateGastoAsync(id, updateDto);
                if (gasto == null)
                {
                    return NotFound(new { message = $"No se encontró el gasto con ID {id}" });
                }

                return Ok(gasto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el gasto con ID {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar el gasto" });
            }
        }

        [HttpGet("listado")]
        public async Task<IActionResult> GetListado([FromQuery] DateTime fechaDesde, [FromQuery] DateTime fechaHasta)
        {
            try
            {
                var gastos = await _gastoService.GetGastosByFechaRangoAsync(fechaDesde, fechaHasta);
                return Ok(gastos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el listado de gastos");
                return StatusCode(500, new { message = "Error al obtener el listado de gastos" });
            }
        }

        [HttpGet("listado-pdf")]
        public async Task<IActionResult> GetListadoPdf([FromQuery] DateTime fechaDesde, [FromQuery] DateTime fechaHasta)
        {
            try
            {
                var gastos = await _gastoService.GetGastosByFechaRangoAsync(fechaDesde, fechaHasta);
                var pdf = _gastoPdfService.GenerarPdf(gastos.ToList(), fechaDesde, fechaHasta);
                return File(pdf, "application/pdf", $"listado-gastos-{fechaDesde:yyyyMMdd}-{fechaHasta:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF del listado de gastos");
                return StatusCode(500, new { message = "Error al generar el PDF del listado de gastos" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _gastoService.DeleteGastoAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"No se encontró el gasto con ID {id}" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el gasto con ID {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar el gasto" });
            }
        }
    }
}
