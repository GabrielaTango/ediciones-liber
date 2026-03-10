using BookstoreAPI.DTOs;
using BookstoreAPI.Services;
using BookstoreAPI.Services.Pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RemitosController : ControllerBase
    {
        private readonly IRemitoService _remitoService;
        private readonly IRemitoPdfService _remitoPdfService;
        private readonly ILogger<RemitosController> _logger;

        public RemitosController(
            IRemitoService remitoService,
            IRemitoPdfService remitoPdfService,
            ILogger<RemitosController> logger)
        {
            _remitoService = remitoService;
            _remitoPdfService = remitoPdfService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var remitos = await _remitoService.GetAllAsync();
                return Ok(remitos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener remitos");
                return StatusCode(500, new { message = "Error al obtener remitos" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var remito = await _remitoService.GetByIdAsync(id);
                if (remito == null)
                    return NotFound(new { message = $"Remito con ID {id} no encontrado" });
                return Ok(remito);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener remito por ID");
                return StatusCode(500, new { message = "Error al obtener remito" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRemitoDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var remito = await _remitoService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = remito.Id }, remito);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear remito");
                return StatusCode(500, new { message = "Error al crear remito" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRemitoDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var remito = await _remitoService.UpdateAsync(id, dto);
                if (remito == null)
                    return NotFound(new { message = $"Remito con ID {id} no encontrado" });
                return Ok(remito);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar remito");
                return StatusCode(500, new { message = "Error al actualizar remito" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _remitoService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Remito con ID {id} no encontrado" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar remito");
                return StatusCode(500, new { message = "Error al eliminar remito" });
            }
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetPdf(int id)
        {
            try
            {
                var remito = await _remitoService.GetByIdAsync(id);
                if (remito == null)
                    return NotFound(new { message = $"Remito con ID {id} no encontrado" });

                var pdfBytes = _remitoPdfService.GenerarRemitoPdf(remito);
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF del remito");
                return StatusCode(500, new { message = "Error al generar PDF del remito" });
            }
        }

        [HttpGet("{id}/etiquetas-pdf")]
        public async Task<IActionResult> GetEtiquetasPdf(int id)
        {
            try
            {
                var remito = await _remitoService.GetByIdAsync(id);
                if (remito == null)
                    return NotFound(new { message = $"Remito con ID {id} no encontrado" });

                var pdfBytes = _remitoPdfService.GenerarEtiquetasPdf(remito);
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de etiquetas");
                return StatusCode(500, new { message = "Error al generar PDF de etiquetas" });
            }
        }

        [HttpGet("{id}/completo-pdf")]
        public async Task<IActionResult> GetCompletoPdf(int id)
        {
            try
            {
                var remito = await _remitoService.GetByIdAsync(id);
                if (remito == null)
                    return NotFound(new { message = $"Remito con ID {id} no encontrado" });

                var pdfBytes = _remitoPdfService.GenerarRemitoCompletoConEtiquetas(remito);
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF completo del remito");
                return StatusCode(500, new { message = "Error al generar PDF completo del remito" });
            }
        }
    }
}
