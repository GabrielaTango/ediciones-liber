using BookstoreAPI.DTOs;
using BookstoreAPI.Repositories;
using BookstoreAPI.Services;
using BookstoreAPI.Services.Pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ComprobantesProveedoresController : ControllerBase
    {
        private readonly IComprobanteProveedorService _service;
        private readonly IComprobanteProveedorRepository _repository;
        private readonly IIvaComprasPdfService _ivaComprasPdfService;
        private readonly ILogger<ComprobantesProveedoresController> _logger;

        public ComprobantesProveedoresController(
            IComprobanteProveedorService service,
            IComprobanteProveedorRepository repository,
            IIvaComprasPdfService ivaComprasPdfService,
            ILogger<ComprobantesProveedoresController> logger)
        {
            _service = service;
            _repository = repository;
            _ivaComprasPdfService = ivaComprasPdfService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var comprobantes = await _service.GetAllAsync();
                return Ok(comprobantes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comprobantes de proveedores");
                return StatusCode(500, new { message = "Error al obtener comprobantes de proveedores" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var comprobante = await _service.GetDetailByIdAsync(id);
                if (comprobante == null)
                    return NotFound(new { message = $"Comprobante de proveedor con ID {id} no encontrado" });
                return Ok(comprobante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comprobante de proveedor por ID");
                return StatusCode(500, new { message = "Error al obtener comprobante de proveedor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateComprobanteProveedorDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var comprobante = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = comprobante.Id }, comprobante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear comprobante de proveedor");
                return StatusCode(500, new { message = "Error al crear comprobante de proveedor", error = ex.Message });
            }
        }

        [HttpGet("iva-compras")]
        public async Task<IActionResult> GetIvaCompras([FromQuery] DateTime fechaDesde, [FromQuery] DateTime fechaHasta)
        {
            try
            {
                var data = await _repository.GetIvaComprasAsync(fechaDesde, fechaHasta);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener IVA compras");
                return StatusCode(500, new { message = "Error al obtener IVA compras" });
            }
        }

        [HttpGet("iva-compras-pdf")]
        public async Task<IActionResult> GetIvaComprasPdf([FromQuery] DateTime fechaDesde, [FromQuery] DateTime fechaHasta)
        {
            try
            {
                if (fechaDesde > fechaHasta)
                    return BadRequest(new { message = "La fecha desde no puede ser mayor que la fecha hasta" });

                var compras = await _repository.GetIvaComprasAsync(fechaDesde, fechaHasta);
                var listaCompras = compras.ToList();

                if (!listaCompras.Any())
                    return NotFound(new { message = "No se encontraron compras en el período especificado" });

                var pdfBytes = _ivaComprasPdfService.GenerarPdf(listaCompras, fechaDesde, fechaHasta);

                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de IVA compras");
                return StatusCode(500, new { message = "Error al generar PDF de IVA compras" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Comprobante de proveedor con ID {id} no encontrado" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar comprobante de proveedor");
                return StatusCode(500, new { message = "Error al eliminar comprobante de proveedor" });
            }
        }
    }
}
