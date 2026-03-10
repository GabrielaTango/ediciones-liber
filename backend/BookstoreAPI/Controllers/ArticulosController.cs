using BookstoreAPI.DTOs;
using BookstoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ArticulosController : ControllerBase
    {
        private readonly IArticuloService _articuloService;
        private readonly ILogger<ArticulosController> _logger;

        public ArticulosController(IArticuloService articuloService, ILogger<ArticulosController> logger)
        {
            _articuloService = articuloService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los artículos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var articulos = await _articuloService.GetAllArticulosAsync();
                return Ok(articulos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los artículos");
                return StatusCode(500, new { message = "Error al obtener los artículos" });
            }
        }

        /// <summary>
        /// Obtiene un artículo por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var articulo = await _articuloService.GetArticuloByIdAsync(id);
                if (articulo == null)
                {
                    return NotFound(new { message = $"Artículo con ID {id} no encontrado" });
                }
                return Ok(articulo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el artículo con ID {Id}", id);
                return StatusCode(500, new { message = "Error al obtener el artículo" });
            }
        }

        /// <summary>
        /// Obtiene un artículo por código
        /// </summary>
        [HttpGet("codigo/{codigo}")]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            try
            {
                var articulo = await _articuloService.GetArticuloByCodigoAsync(codigo);
                if (articulo == null)
                {
                    return NotFound(new { message = $"Artículo con código {codigo} no encontrado" });
                }
                return Ok(articulo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el artículo con código {Codigo}", codigo);
                return StatusCode(500, new { message = "Error al obtener el artículo" });
            }
        }

        /// <summary>
        /// Crea un nuevo artículo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateArticuloDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var articulo = await _articuloService.CreateArticuloAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = articulo.Id }, articulo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el artículo");
                return StatusCode(500, new { message = "Error al crear el artículo" });
            }
        }

        /// <summary>
        /// Actualiza un artículo existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateArticuloDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var articulo = await _articuloService.UpdateArticuloAsync(id, updateDto);
                if (articulo == null)
                {
                    return NotFound(new { message = $"Artículo con ID {id} no encontrado" });
                }

                return Ok(articulo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el artículo con ID {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar el artículo" });
            }
        }

        /// <summary>
        /// Elimina un artículo
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _articuloService.DeleteArticuloAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"Artículo con ID {id} no encontrado" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el artículo con ID {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar el artículo" });
            }
        }
    }
}
