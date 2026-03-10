using BookstoreAPI.DTOs;
using BookstoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProveedoresController : ControllerBase
    {
        private readonly IProveedorService _proveedorService;
        private readonly ILogger<ProveedoresController> _logger;

        public ProveedoresController(IProveedorService proveedorService, ILogger<ProveedoresController> logger)
        {
            _proveedorService = proveedorService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var proveedores = await _proveedorService.GetAllProveedoresAsync();
                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los proveedores");
                return StatusCode(500, new { message = "Error al obtener los proveedores" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var proveedor = await _proveedorService.GetProveedorByIdAsync(id);
                if (proveedor == null)
                {
                    return NotFound(new { message = $"Proveedor con ID {id} no encontrado" });
                }
                return Ok(proveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el proveedor con ID {Id}", id);
                return StatusCode(500, new { message = "Error al obtener el proveedor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProveedorDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var proveedor = await _proveedorService.CreateProveedorAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = proveedor.Id }, proveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el proveedor");
                return StatusCode(500, new { message = "Error al crear el proveedor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProveedorDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var proveedor = await _proveedorService.UpdateProveedorAsync(id, updateDto);
                if (proveedor == null)
                {
                    return NotFound(new { message = $"Proveedor con ID {id} no encontrado" });
                }

                return Ok(proveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el proveedor con ID {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar el proveedor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _proveedorService.DeleteProveedorAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"Proveedor con ID {id} no encontrado" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el proveedor con ID {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar el proveedor" });
            }
        }
    }
}
