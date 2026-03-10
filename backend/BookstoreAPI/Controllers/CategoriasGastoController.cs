using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookstoreAPI.DTOs;
using BookstoreAPI.Services;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriasGastoController : ControllerBase
    {
        private readonly ICategoriaGastoService _categoriaService;
        private readonly ILogger<CategoriasGastoController> _logger;

        public CategoriasGastoController(ICategoriaGastoService categoriaService, ILogger<CategoriasGastoController> logger)
        {
            _categoriaService = categoriaService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categorias = await _categoriaService.GetAllCategoriasAsync();
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las categorías de gasto");
                return StatusCode(500, new { message = "Error al obtener las categorías" });
            }
        }

        [HttpGet("activas")]
        public async Task<IActionResult> GetActivas()
        {
            try
            {
                var categorias = await _categoriaService.GetCategoriasActivasAsync();
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las categorías activas");
                return StatusCode(500, new { message = "Error al obtener las categorías" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var categoria = await _categoriaService.GetCategoriaByIdAsync(id);
                if (categoria == null)
                {
                    return NotFound(new { message = $"No se encontró la categoría con ID {id}" });
                }
                return Ok(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la categoría con ID {Id}", id);
                return StatusCode(500, new { message = "Error al obtener la categoría" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoriaGastoDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var categoria = await _categoriaService.CreateCategoriaAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la categoría");
                return StatusCode(500, new { message = "Error al crear la categoría" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoriaGastoDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var categoria = await _categoriaService.UpdateCategoriaAsync(id, updateDto);
                if (categoria == null)
                {
                    return NotFound(new { message = $"No se encontró la categoría con ID {id}" });
                }

                return Ok(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la categoría con ID {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar la categoría" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _categoriaService.DeleteCategoriaAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"No se encontró la categoría con ID {id}" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la categoría con ID {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar la categoría" });
            }
        }
    }
}
