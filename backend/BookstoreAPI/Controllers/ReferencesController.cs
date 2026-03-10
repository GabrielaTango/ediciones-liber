using BookstoreAPI.DTOs;
using BookstoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReferencesController : ControllerBase
    {
        private readonly IReferenceService _referenceService;
        private readonly ILogger<ReferencesController> _logger;

        public ReferencesController(IReferenceService referenceService, ILogger<ReferencesController> logger)
        {
            _referenceService = referenceService;
            _logger = logger;
        }

        // ===== ZONAS =====
        [HttpGet("zonas")]
        public async Task<IActionResult> GetZonas()
        {
            try
            {
                var zonas = await _referenceService.GetAllZonasAsync();
                return Ok(zonas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener zonas");
                return StatusCode(500, new { message = "Error al obtener zonas" });
            }
        }

        [HttpGet("zonas/{id}")]
        public async Task<IActionResult> GetZonaById(int id)
        {
            try
            {
                var zona = await _referenceService.GetZonaByIdAsync(id);
                if (zona == null)
                    return NotFound(new { message = $"Zona con ID {id} no encontrada" });
                return Ok(zona);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener zona por ID");
                return StatusCode(500, new { message = "Error al obtener zona" });
            }
        }

        [HttpPost("zonas")]
        public async Task<IActionResult> CreateZona([FromBody] CreateZonaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var zona = await _referenceService.CreateZonaAsync(dto);
                return CreatedAtAction(nameof(GetZonaById), new { id = zona.Id }, zona);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear zona");
                return StatusCode(500, new { message = "Error al crear zona" });
            }
        }

        [HttpPut("zonas/{id}")]
        public async Task<IActionResult> UpdateZona(int id, [FromBody] UpdateZonaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var zona = await _referenceService.UpdateZonaAsync(id, dto);
                if (zona == null)
                    return NotFound(new { message = $"Zona con ID {id} no encontrada" });
                return Ok(zona);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar zona");
                return StatusCode(500, new { message = "Error al actualizar zona" });
            }
        }

        [HttpDelete("zonas/{id}")]
        public async Task<IActionResult> DeleteZona(int id)
        {
            try
            {
                var deleted = await _referenceService.DeleteZonaAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Zona con ID {id} no encontrada" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar zona");
                return StatusCode(500, new { message = "Error al eliminar zona" });
            }
        }

        // ===== SUBZONAS =====
        [HttpGet("subzonas")]
        public async Task<IActionResult> GetSubZonas()
        {
            try
            {
                var subzonas = await _referenceService.GetAllSubZonasAsync();
                return Ok(subzonas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener subzonas");
                return StatusCode(500, new { message = "Error al obtener subzonas" });
            }
        }

        [HttpGet("subzonas/{id}")]
        public async Task<IActionResult> GetSubZonaById(int id)
        {
            try
            {
                var subzona = await _referenceService.GetSubZonaByIdAsync(id);
                if (subzona == null)
                    return NotFound(new { message = $"SubZona con ID {id} no encontrada" });
                return Ok(subzona);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener subzona por ID");
                return StatusCode(500, new { message = "Error al obtener subzona" });
            }
        }

        [HttpPost("subzonas")]
        public async Task<IActionResult> CreateSubZona([FromBody] CreateSubZonaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var subzona = await _referenceService.CreateSubZonaAsync(dto);
                return CreatedAtAction(nameof(GetSubZonaById), new { id = subzona.Id }, subzona);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear subzona");
                return StatusCode(500, new { message = "Error al crear subzona" });
            }
        }

        [HttpPut("subzonas/{id}")]
        public async Task<IActionResult> UpdateSubZona(int id, [FromBody] UpdateSubZonaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var subzona = await _referenceService.UpdateSubZonaAsync(id, dto);
                if (subzona == null)
                    return NotFound(new { message = $"SubZona con ID {id} no encontrada" });
                return Ok(subzona);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar subzona");
                return StatusCode(500, new { message = "Error al actualizar subzona" });
            }
        }

        [HttpDelete("subzonas/{id}")]
        public async Task<IActionResult> DeleteSubZona(int id)
        {
            try
            {
                var deleted = await _referenceService.DeleteSubZonaAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"SubZona con ID {id} no encontrada" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar subzona");
                return StatusCode(500, new { message = "Error al eliminar subzona" });
            }
        }

        // ===== PROVINCIAS =====
        [HttpGet("provincias")]
        public async Task<IActionResult> GetProvincias()
        {
            try
            {
                var provincias = await _referenceService.GetAllProvinciasAsync();
                return Ok(provincias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener provincias");
                return StatusCode(500, new { message = "Error al obtener provincias" });
            }
        }

        [HttpGet("provincias/{id}")]
        public async Task<IActionResult> GetProvinciaById(int id)
        {
            try
            {
                var provincia = await _referenceService.GetProvinciaByIdAsync(id);
                if (provincia == null)
                    return NotFound(new { message = $"Provincia con ID {id} no encontrada" });
                return Ok(provincia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener provincia por ID");
                return StatusCode(500, new { message = "Error al obtener provincia" });
            }
        }

        [HttpPost("provincias")]
        public async Task<IActionResult> CreateProvincia([FromBody] CreateProvinciaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var provincia = await _referenceService.CreateProvinciaAsync(dto);
                return CreatedAtAction(nameof(GetProvinciaById), new { id = provincia.Id }, provincia);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear provincia");
                return StatusCode(500, new { message = "Error al crear provincia" });
            }
        }

        [HttpPut("provincias/{id}")]
        public async Task<IActionResult> UpdateProvincia(int id, [FromBody] UpdateProvinciaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var provincia = await _referenceService.UpdateProvinciaAsync(id, dto);
                if (provincia == null)
                    return NotFound(new { message = $"Provincia con ID {id} no encontrada" });
                return Ok(provincia);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar provincia");
                return StatusCode(500, new { message = "Error al actualizar provincia" });
            }
        }

        [HttpDelete("provincias/{id}")]
        public async Task<IActionResult> DeleteProvincia(int id)
        {
            try
            {
                var deleted = await _referenceService.DeleteProvinciaAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Provincia con ID {id} no encontrada" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar provincia");
                return StatusCode(500, new { message = "Error al eliminar provincia" });
            }
        }

        // ===== VENDEDORES =====
        [HttpGet("vendedores")]
        public async Task<IActionResult> GetVendedores()
        {
            try
            {
                var vendedores = await _referenceService.GetAllVendedoresAsync();
                return Ok(vendedores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vendedores");
                return StatusCode(500, new { message = "Error al obtener vendedores" });
            }
        }

        [HttpGet("vendedores/{id}")]
        public async Task<IActionResult> GetVendedorById(int id)
        {
            try
            {
                var vendedor = await _referenceService.GetVendedorByIdAsync(id);
                if (vendedor == null)
                    return NotFound(new { message = $"Vendedor con ID {id} no encontrado" });
                return Ok(vendedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vendedor por ID");
                return StatusCode(500, new { message = "Error al obtener vendedor" });
            }
        }

        [HttpPost("vendedores")]
        public async Task<IActionResult> CreateVendedor([FromBody] CreateVendedorDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var vendedor = await _referenceService.CreateVendedorAsync(dto);
                return CreatedAtAction(nameof(GetVendedorById), new { id = vendedor.Id }, vendedor);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear vendedor");
                return StatusCode(500, new { message = "Error al crear vendedor" });
            }
        }

        [HttpPut("vendedores/{id}")]
        public async Task<IActionResult> UpdateVendedor(int id, [FromBody] UpdateVendedorDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var vendedor = await _referenceService.UpdateVendedorAsync(id, dto);
                if (vendedor == null)
                    return NotFound(new { message = $"Vendedor con ID {id} no encontrado" });
                return Ok(vendedor);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar vendedor");
                return StatusCode(500, new { message = "Error al actualizar vendedor" });
            }
        }

        [HttpDelete("vendedores/{id}")]
        public async Task<IActionResult> DeleteVendedor(int id)
        {
            try
            {
                var deleted = await _referenceService.DeleteVendedorAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Vendedor con ID {id} no encontrado" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar vendedor");
                return StatusCode(500, new { message = "Error al eliminar vendedor" });
            }
        }

        // ===== TRANSPORTES =====
        [HttpGet("transportes")]
        public async Task<IActionResult> GetTransportes()
        {
            try
            {
                var transportes = await _referenceService.GetAllTransportesAsync();
                return Ok(transportes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transportes");
                return StatusCode(500, new { message = "Error al obtener transportes" });
            }
        }

        [HttpGet("transportes/{id}")]
        public async Task<IActionResult> GetTransporteById(int id)
        {
            try
            {
                var transporte = await _referenceService.GetTransporteByIdAsync(id);
                if (transporte == null)
                    return NotFound(new { message = $"Transporte con ID {id} no encontrado" });
                return Ok(transporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transporte por ID");
                return StatusCode(500, new { message = "Error al obtener transporte" });
            }
        }

        [HttpPost("transportes")]
        public async Task<IActionResult> CreateTransporte([FromBody] CreateTransporteDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var transporte = await _referenceService.CreateTransporteAsync(dto);
                return CreatedAtAction(nameof(GetTransporteById), new { id = transporte.Id }, transporte);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear transporte");
                return StatusCode(500, new { message = "Error al crear transporte" });
            }
        }

        [HttpPut("transportes/{id}")]
        public async Task<IActionResult> UpdateTransporte(int id, [FromBody] UpdateTransporteDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var transporte = await _referenceService.UpdateTransporteAsync(id, dto);
                if (transporte == null)
                    return NotFound(new { message = $"Transporte con ID {id} no encontrado" });
                return Ok(transporte);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar transporte");
                return StatusCode(500, new { message = "Error al actualizar transporte" });
            }
        }

        [HttpDelete("transportes/{id}")]
        public async Task<IActionResult> DeleteTransporte(int id)
        {
            try
            {
                var deleted = await _referenceService.DeleteTransporteAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Transporte con ID {id} no encontrado" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar transporte");
                return StatusCode(500, new { message = "Error al eliminar transporte" });
            }
        }
    }
}
