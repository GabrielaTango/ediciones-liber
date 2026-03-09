using BookstoreAPI.DTOs;
using BookstoreAPI.Repositories;
using BookstoreAPI.Services;
using BookstoreAPI.Services.Pdf;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComprobantesController : ControllerBase
    {
        private readonly IComprobanteService _comprobanteService;
        private readonly IComprobantePdfService _pdfService;
        private readonly ICuotaPdfService _cuotaPdfService;
        private readonly IIvaVentasPdfService _ivaVentasPdfService;
        private readonly IDeudoresPdfService _deudoresPdfService;
        private readonly IComprobanteRepository _comprobanteRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly ICuotaRepository _cuotaRepository;
        private readonly ILogger<ComprobantesController> _logger;

        public ComprobantesController(
            IComprobanteService comprobanteService,
            IComprobantePdfService pdfService,
            ICuotaPdfService cuotaPdfService,
            IIvaVentasPdfService ivaVentasPdfService,
            IDeudoresPdfService deudoresPdfService,
            IComprobanteRepository comprobanteRepository,
            IClienteRepository clienteRepository,
            ICuotaRepository cuotaRepository,
            ILogger<ComprobantesController> logger)
        {
            _comprobanteService = comprobanteService;
            _pdfService = pdfService;
            _cuotaPdfService = cuotaPdfService;
            _ivaVentasPdfService = ivaVentasPdfService;
            _deudoresPdfService = deudoresPdfService;
            _comprobanteRepository = comprobanteRepository;
            _clienteRepository = clienteRepository;
            _cuotaRepository = cuotaRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? zonaId,
            [FromQuery] int? clienteId,
            [FromQuery] string? tipoComprobante,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] int? vendedorId,
            [FromQuery] string? comprobante)
        {
            try
            {
                // Si hay algún filtro, usar el método filtrado
                if (zonaId.HasValue || clienteId.HasValue || !string.IsNullOrWhiteSpace(tipoComprobante) || fechaDesde.HasValue || fechaHasta.HasValue || vendedorId.HasValue || !string.IsNullOrWhiteSpace(comprobante))
                {
                    var comprobantesFiltrados = await _comprobanteRepository.GetAllFilteredAsync(
                        zonaId, clienteId, tipoComprobante, fechaDesde, fechaHasta, vendedorId, comprobante);
                    return Ok(comprobantesFiltrados);
                }

                var comprobantes = await _comprobanteService.GetAllAsync();
                return Ok(comprobantes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comprobantes");
                return StatusCode(500, new { message = "Error al obtener comprobantes" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var comprobante = await _comprobanteService.GetByIdAsync(id);
                if (comprobante == null)
                    return NotFound(new { message = $"Comprobante con ID {id} no encontrado" });
                return Ok(comprobante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comprobante por ID");
                return StatusCode(500, new { message = "Error al obtener comprobante" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateComprobanteDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var comprobante = await _comprobanteService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = comprobante.Id }, comprobante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear comprobante");
                return StatusCode(500, new { message = "Error al crear comprobante", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateComprobanteDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var comprobante = await _comprobanteService.UpdateAsync(id, dto);
                if (comprobante == null)
                    return NotFound(new { message = $"Comprobante con ID {id} no encontrado" });
                return Ok(comprobante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar comprobante");
                return StatusCode(500, new { message = "Error al actualizar comprobante", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _comprobanteService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Comprobante con ID {id} no encontrado" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar comprobante");
                return StatusCode(500, new { message = "Error al eliminar comprobante" });
            }
        }

        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var notaCredito = await _comprobanteService.CancelarAsync(id);
                return Ok(notaCredito);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar comprobante {Id}", id);
                return StatusCode(500, new { message = "Error al cancelar comprobante", error = ex.Message });
            }
        }

        [HttpPost("{id}/cancelar-deuda")]
        public async Task<IActionResult> CancelarDeuda(int id)
        {
            try
            {
                // Obtener el comprobante
                var comprobante = await _comprobanteRepository.GetByIdAsync(id);
                if (comprobante == null)
                    return NotFound(new { message = $"Comprobante con ID {id} no encontrado" });

                // Validar que esté en estado PEN
                if (comprobante.Estado != "PEN")
                    return BadRequest(new { message = $"Solo se puede cancelar la deuda de comprobantes en estado Pendiente. Estado actual: {comprobante.Estado}" });

                // Eliminar cuotas pendientes (no pagadas)
                await _cuotaRepository.DeletePendientesByComprobanteIdAsync(id);

                // Actualizar estado del comprobante a CAN
                await _comprobanteRepository.UpdateEstadoAsync(id, "CAN");

                return Ok(new { message = "Deuda cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar deuda del comprobante {Id}", id);
                return StatusCode(500, new { message = "Error al cancelar deuda del comprobante", error = ex.Message });
            }
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetPdf(int id)
        {
            try
            {
                // Obtener comprobante
                var comprobante = await _comprobanteRepository.GetComprobanteByIdAsync(id);
                if (comprobante == null)
                    return NotFound(new { message = $"Comprobante con ID {id} no encontrado" });

                // Obtener cliente
                var cliente = await _clienteRepository.GetByIdAsync(comprobante.Cliente_Id);
                if (cliente == null)
                    return NotFound(new { message = "Cliente no encontrado" });

                // Obtener detalles del comprobante
                var detalles = await _comprobanteRepository.GetDetallesByComprobanteIdAsync(id);

                // Generar PDF
                var pdfBytes = _pdfService.GenerarPdf(comprobante, cliente, detalles);

                // Retornar PDF para abrir en el navegador
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF del comprobante {Id}", id);
                return StatusCode(500, new { message = "Error al generar PDF del comprobante", error = ex.Message });
            }
        }

        [HttpGet("{id}/cupones-pdf")]
        public async Task<IActionResult> GetCuponesPdf(int id)
        {
            try
            {
                // Obtener comprobante
                var comprobante = await _comprobanteRepository.GetComprobanteByIdAsync(id);
                if (comprobante == null)
                    return NotFound(new { message = $"Comprobante con ID {id} no encontrado" });

                // Obtener cliente
                var cliente = await _clienteRepository.GetByIdAsync(comprobante.Cliente_Id);
                if (cliente == null)
                    return NotFound(new { message = "Cliente no encontrado" });

                // Obtener cuotas del comprobante
                var cuotas = await _cuotaRepository.GetByComprobanteIdAsync(id);
                var listaCuotas = cuotas.ToList();

                if (!listaCuotas.Any())
                    return NotFound(new { message = "No hay cuotas para este comprobante" });

                // Generar PDF de cupones
                var pdfBytes = _cuotaPdfService.GenerarCuponesPdf(comprobante, cliente, listaCuotas);

                // Retornar PDF para abrir en el navegador
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de cupones del comprobante {Id}", id);
                return StatusCode(500, new { message = "Error al generar PDF de cupones", error = ex.Message });
            }
        }

        [HttpGet("ultimo-gasto-envio")]
        public async Task<IActionResult> GetUltimoGastoEnvio()
        {
            try
            {
                var gastoEnvio = await _comprobanteRepository.GetUltimoGastoEnvioAsync();
                return Ok(new { gastoEnvio });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener último gasto de envío");
                return StatusCode(500, new { message = "Error al obtener último gasto de envío" });
            }
        }

        [HttpGet("iva-ventas")]
        public async Task<IActionResult> GetIvaVentas([FromQuery] DateTime fechaDesde, [FromQuery] DateTime fechaHasta)
        {
            try
            {
                // Validar que la fecha desde no sea mayor que la fecha hasta
                if (fechaDesde > fechaHasta)
                    return BadRequest(new { message = "La fecha desde no puede ser mayor que la fecha hasta" });

                // Obtener datos de IVA ventas
                var ventas = await _comprobanteRepository.GetIvaVentasAsync(fechaDesde, fechaHasta);
                return Ok(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos de IVA ventas");
                return StatusCode(500, new { message = "Error al obtener datos de IVA ventas", error = ex.Message });
            }
        }

        [HttpGet("iva-ventas-pdf")]
        public async Task<IActionResult> GetIvaVentasPdf([FromQuery] DateTime fechaDesde, [FromQuery] DateTime fechaHasta)
        {
            try
            {
                // Validar que la fecha desde no sea mayor que la fecha hasta
                if (fechaDesde > fechaHasta)
                    return BadRequest(new { message = "La fecha desde no puede ser mayor que la fecha hasta" });

                // Obtener datos de IVA ventas
                var ventas = await _comprobanteRepository.GetIvaVentasAsync(fechaDesde, fechaHasta);
                var listaVentas = ventas.ToList();

                if (!listaVentas.Any())
                    return NotFound(new { message = "No se encontraron ventas en el período especificado" });

                // Generar PDF
                var pdfBytes = _ivaVentasPdfService.GenerarPdf(listaVentas, fechaDesde, fechaHasta);

                // Retornar PDF para abrir en el navegador
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de IVA ventas");
                return StatusCode(500, new { message = "Error al generar PDF de IVA ventas", error = ex.Message });
            }
        }

        [HttpGet("deudores")]
        public async Task<IActionResult> GetDeudores([FromQuery] int mes, [FromQuery] int anio, [FromQuery] int? zonaId, [FromQuery] int? vendedorId)
        {
            try
            {
                // Validar mes y año
                if (mes < 1 || mes > 12)
                    return BadRequest(new { message = "El mes debe estar entre 1 y 12" });

                if (anio < 2000 || anio > 2100)
                    return BadRequest(new { message = "El año debe estar entre 2000 y 2100" });

                // Obtener datos de deudores
                var deudores = await _comprobanteRepository.GetDeudoresAsync(mes, anio, zonaId, vendedorId);
                return Ok(deudores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos de deudores");
                return StatusCode(500, new { message = "Error al obtener datos de deudores", error = ex.Message });
            }
        }

        [HttpGet("deudores-pdf")]
        public async Task<IActionResult> GetDeudoresPdf([FromQuery] int mes, [FromQuery] int anio, [FromQuery] int? zonaId, [FromQuery] int? vendedorId)
        {
            try
            {
                if (mes < 1 || mes > 12)
                    return BadRequest(new { message = "El mes debe estar entre 1 y 12" });

                if (anio < 2000 || anio > 2100)
                    return BadRequest(new { message = "El año debe estar entre 2000 y 2100" });

                var reporte = await _comprobanteRepository.GetDeudoresAsync(mes, anio, zonaId, vendedorId);
                var pdf = _deudoresPdfService.GenerarPdf(reporte);
                return File(pdf, "application/pdf", $"deudores-{mes:D2}-{anio}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de deudores");
                return StatusCode(500, new { message = "Error al generar PDF de deudores", error = ex.Message });
            }
        }

        [HttpGet("{id}/completo-pdf")]
        public async Task<IActionResult> GetCompletoPdf(int id)
        {
            try
            {
                // Obtener comprobante
                var comprobante = await _comprobanteRepository.GetComprobanteByIdAsync(id);
                if (comprobante == null)
                    return NotFound(new { message = $"Comprobante con ID {id} no encontrado" });

                // Obtener cliente
                var cliente = await _clienteRepository.GetByIdAsync(comprobante.Cliente_Id);
                if (cliente == null)
                    return NotFound(new { message = "Cliente no encontrado" });

                // Obtener detalles del comprobante
                var detalles = await _comprobanteRepository.GetDetallesByComprobanteIdAsync(id);

                // Obtener cuotas del comprobante
                var cuotas = await _cuotaRepository.GetByComprobanteIdAsync(id);
                var listaCuotas = cuotas.ToList();

                // Generar PDF completo (comprobante x3 + cupones)
                var pdfBytes = _pdfService.GenerarComprobanteCompletoConCupones(comprobante, cliente, detalles, listaCuotas);

                // Retornar PDF para abrir en el navegador
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF completo del comprobante {Id}", id);
                return StatusCode(500, new { message = "Error al generar PDF completo del comprobante", error = ex.Message });
            }
        }

        [HttpGet("batch-pdf")]
        public async Task<IActionResult> GetBatchPdf(
            [FromQuery] int? zonaId,
            [FromQuery] int? clienteId,
            [FromQuery] string? tipoComprobante,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] int? vendedorId)
        {
            try
            {
                if (!zonaId.HasValue && !clienteId.HasValue && string.IsNullOrWhiteSpace(tipoComprobante) && !fechaDesde.HasValue && !fechaHasta.HasValue && !vendedorId.HasValue)
                    return BadRequest(new { message = "Debe aplicar al menos un filtro para imprimir en lote" });

                var comprobantesFiltrados = await _comprobanteRepository.GetAllFilteredAsync(
                    zonaId, clienteId, tipoComprobante, fechaDesde, fechaHasta, vendedorId);

                var lista = comprobantesFiltrados.ToList();
                if (!lista.Any())
                    return NotFound(new { message = "No se encontraron comprobantes con los filtros aplicados" });

                var lote = new List<(Models.Comprobante comprobante, Models.Cliente cliente, List<Models.ComprobanteDetalle> detalles)>();

                foreach (var comp in lista)
                {
                    var comprobante = await _comprobanteRepository.GetComprobanteByIdAsync(comp.Id);
                    if (comprobante == null) continue;

                    var cliente = await _clienteRepository.GetByIdAsync(comprobante.Cliente_Id);
                    if (cliente == null) continue;

                    var detalles = await _comprobanteRepository.GetDetallesByComprobanteIdAsync(comp.Id);
                    lote.Add((comprobante, cliente, detalles));
                }

                if (!lote.Any())
                    return NotFound(new { message = "No se pudieron obtener los datos de los comprobantes" });

                lote = lote.OrderBy(x => x.comprobante.TipoComprobante switch
                {
                    "PRE" => 0,
                    "FC" => 1,
                    _ => 2
                }).ToList();

                var pdfBytes = _pdfService.GenerarLotePdf(lote);
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF en lote de comprobantes");
                return StatusCode(500, new { message = "Error al generar PDF en lote", error = ex.Message });
            }
        }

        [HttpGet("batch-cupones-pdf")]
        public async Task<IActionResult> GetBatchCuponesPdf(
            [FromQuery] int? zonaId,
            [FromQuery] int? clienteId,
            [FromQuery] string? tipoComprobante,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] int? vendedorId)
        {
            try
            {
                if (!zonaId.HasValue && !clienteId.HasValue && string.IsNullOrWhiteSpace(tipoComprobante) && !fechaDesde.HasValue && !fechaHasta.HasValue && !vendedorId.HasValue)
                    return BadRequest(new { message = "Debe aplicar al menos un filtro para imprimir cupones en lote" });

                var comprobantesFiltrados = await _comprobanteRepository.GetAllFilteredAsync(
                    zonaId, clienteId, tipoComprobante, fechaDesde, fechaHasta, vendedorId);

                var lista = comprobantesFiltrados.ToList();
                if (!lista.Any())
                    return NotFound(new { message = "No se encontraron comprobantes con los filtros aplicados" });

                var lote = new List<(Models.Comprobante comprobante, Models.Cliente cliente, List<Models.Cuota> cuotas)>();

                foreach (var comp in lista)
                {
                    var comprobante = await _comprobanteRepository.GetComprobanteByIdAsync(comp.Id);
                    if (comprobante == null) continue;

                    var cliente = await _clienteRepository.GetByIdAsync(comprobante.Cliente_Id);
                    if (cliente == null) continue;

                    var cuotas = await _cuotaRepository.GetByComprobanteIdAsync(comp.Id);
                    var listaCuotas = cuotas.ToList();

                    if (listaCuotas.Any())
                        lote.Add((comprobante, cliente, listaCuotas));
                }

                if (!lote.Any())
                    return NotFound(new { message = "No se encontraron cupones para los comprobantes filtrados" });

                lote = lote.OrderBy(x => x.comprobante.TipoComprobante switch
                {
                    "PRE" => 0,
                    "FC" => 1,
                    _ => 2
                }).ToList();

                var pdfBytes = _cuotaPdfService.GenerarLoteCuponesPdf(lote);
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de cupones en lote");
                return StatusCode(500, new { message = "Error al generar PDF de cupones en lote", error = ex.Message });
            }
        }

        [HttpGet("articulos-vendidos-zona")]
        public async Task<IActionResult> GetArticulosVendidosPorZona([FromQuery] int? zonaId)
        {
            try
            {
                var reporte = await _comprobanteRepository.GetArticulosVendidosPorZonaAsync(zonaId);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener artículos vendidos por zona");
                return StatusCode(500, new { message = "Error al obtener artículos vendidos por zona", error = ex.Message });
            }
        }

        [HttpGet("articulos-vendidos-zona-pdf")]
        public async Task<IActionResult> GetArticulosVendidosPorZonaPdf([FromQuery] int? zonaId, [FromServices] IArticulosVendidosZonaPdfService articulosZonaPdfService)
        {
            try
            {
                var reporte = await _comprobanteRepository.GetArticulosVendidosPorZonaAsync(zonaId);

                if (!reporte.Items.Any())
                    return NotFound(new { message = "No se encontraron artículos vendidos en el período especificado" });

                var pdfBytes = articulosZonaPdfService.GenerarPdf(reporte);
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de artículos vendidos por zona");
                return StatusCode(500, new { message = "Error al generar PDF de artículos vendidos por zona", error = ex.Message });
            }
        }
    }
}
