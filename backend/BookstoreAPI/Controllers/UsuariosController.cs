using BookstoreAPI.DTOs;
using BookstoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class UsuariosController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(IAuthService authService, ILogger<UsuariosController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var usuarios = await _authService.GetAllUsuariosAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, new { message = "Error al obtener usuarios" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var usuario = await _authService.GetUsuarioByIdAsync(id);
                if (usuario == null)
                    return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con ID {Id}", id);
                return StatusCode(500, new { message = "Error al obtener el usuario" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuario = await _authService.CreateUsuarioAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, new { message = "Error al crear el usuario" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuario = await _authService.UpdateUsuarioAsync(id, updateDto);
                if (usuario == null)
                    return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar el usuario" });
            }
        }

        [HttpPut("{id}/password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.ChangePasswordAsync(id, changePasswordDto);
                if (!result)
                    return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

                return Ok(new { message = "Contraseña actualizada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña del usuario con ID {Id}", id);
                return StatusCode(500, new { message = "Error al cambiar la contraseña" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _authService.DeleteUsuarioAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar el usuario" });
            }
        }
    }
}
