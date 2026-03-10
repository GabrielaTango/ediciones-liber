using BookstoreAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardRepository dashboardRepository,
            ILogger<DashboardController> logger)
        {
            _dashboardRepository = dashboardRepository;
            _logger = logger;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _dashboardRepository.GetStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del dashboard");
                return StatusCode(500, new { message = "Error al obtener estadísticas del dashboard" });
            }
        }

        [HttpGet("actividades-recientes")]
        public async Task<IActionResult> GetActividadesRecientes([FromQuery] int cantidad = 10)
        {
            try
            {
                var actividades = await _dashboardRepository.GetActividadesRecientesAsync(cantidad);
                return Ok(actividades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividades recientes del dashboard");
                return StatusCode(500, new { message = "Error al obtener actividades recientes del dashboard" });
            }
        }
    }
}
