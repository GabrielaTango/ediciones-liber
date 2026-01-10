using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using BookstoreAPI.Models.Afip;
using BookstoreAPI.Repositories;
using BookstoreAPI.Services.Afip;
using Microsoft.Extensions.Options;

namespace BookstoreAPI.Services
{
    public class ComprobanteService : IComprobanteService
    {
        private readonly IComprobanteRepository _comprobanteRepository;
        private readonly ICuotaRepository _cuotaRepository;
        private readonly IAfipFacturacionService _afipService;
        private readonly ILogger<ComprobanteService> _logger;
        private readonly IOptions<AfipConfig> _config;
        public ComprobanteService(
            IComprobanteRepository comprobanteRepository,
            ICuotaRepository cuotaRepository,
            IAfipFacturacionService afipService,
            ILogger<ComprobanteService> logger,
            IOptions<AfipConfig> config)
        {
            _comprobanteRepository = comprobanteRepository;
            _cuotaRepository = cuotaRepository;
            _afipService = afipService;
            _logger = logger;
            _config = config;
        }

        public async Task<IEnumerable<ComprobanteConDetallesDto>> GetAllAsync()
        {
            return await _comprobanteRepository.GetAllAsync();
        }

        public async Task<ComprobanteConDetallesDto?> GetByIdAsync(int id)
        {
            return await _comprobanteRepository.GetByIdAsync(id);
        }

        public async Task<ComprobanteConDetallesDto> CreateAsync(CreateComprobanteDto dto)
        {
            var comprobante = new Comprobante
            {
                Cliente_Id = dto.Cliente_Id,
                Fecha = dto.Fecha,
                TipoComprobante = dto.TipoComprobante,
                Total = dto.Total,
                Bonificacion = dto.Bonificacion,
                PorcentajeBonif = dto.PorcentajeBonif,
                Anticipo = dto.Anticipo,
                ContraEntrega = dto.ContraEntrega,
                Cuotas = dto.Cuotas,
                ValorCuota = dto.ValorCuota,
                Vendedor_Id = dto.Vendedor_Id,
                GastosEnvio = dto.GastosEnvio,
                EsElectronica = dto.EsElectronica,
                EsPresupuesto = dto.EsPresupuesto
            };

            var detalles = dto.Detalles.Select(d => new ComprobanteDetalle
            {
                Articulo_Id = d.Articulo_Id,
                Cantidad = d.Cantidad,
                Precio_Unitario = d.Precio_Unitario,
                Subtotal = d.Subtotal
            }).ToList();

            if (dto.EsElectronica)
            {
                // Factura Electrónica: Solicitar CAE a AFIP
                try
                {
                    _logger.LogInformation("Solicitando CAE a AFIP para comprobante electrónico");
                    var caeResponse = await _afipService.SolicitarCAEAsync(comprobante, detalles);

                    if (caeResponse.Success)
                    {
                        comprobante.CAE = caeResponse.CAE;
                        comprobante.VTO = caeResponse.CAEVencimiento;
                        comprobante.NumeroComprobante = caeResponse.NumeroComprobante;
                        comprobante.TipoComprobante = "FC";
                        _logger.LogInformation("CAE obtenido exitosamente: {CAE}", caeResponse.CAE);
                    }
                    else
                    {
                        var errores = string.Join(", ", caeResponse.Errores);
                        _logger.LogError("Error al obtener CAE de AFIP: {Errores}", errores);
                        throw new Exception($"Error al obtener CAE de AFIP: {errores}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al solicitar CAE a AFIP");
                    throw new Exception($"Error al solicitar CAE: {ex.Message}", ex);
                }
            }
            else
            {
                // Presupuesto: generar número sin CAE
                var puntoVenta = _config.Value.PuntoVenta.ToString("D4");
                comprobante.NumeroComprobante = await _comprobanteRepository.GetSiguienteNumeroPresupuestoAsync(puntoVenta);
                comprobante.TipoComprobante = "PRE";
                comprobante.CAE = null;
                comprobante.VTO = null;
                comprobante.EsPresupuesto = true;
                _logger.LogInformation("Presupuesto generado: {Numero}", comprobante.NumeroComprobante);
            }

            var createdComprobante = await _comprobanteRepository.CreateAsync(comprobante, detalles);

            return await _comprobanteRepository.GetByIdAsync(createdComprobante.Id)
                ?? throw new Exception("Error al recuperar el comprobante creado");
        }

        public async Task<ComprobanteConDetallesDto?> UpdateAsync(int id, UpdateComprobanteDto dto)
        {
            var comprobante = new Comprobante
            {
                Cliente_Id = dto.Cliente_Id,
                Fecha = dto.Fecha,
                TipoComprobante = dto.TipoComprobante,
                NumeroComprobante = dto.NumeroComprobante,
                Total = dto.Total,
                CAE = dto.CAE,
                VTO = dto.VTO,
                Bonificacion = dto.Bonificacion,
                PorcentajeBonif = dto.PorcentajeBonif,
                Anticipo = dto.Anticipo,
                ContraEntrega = dto.ContraEntrega,
                Cuotas = dto.Cuotas,
                ValorCuota = dto.ValorCuota,
                Vendedor_Id = dto.Vendedor_Id,
                GastosEnvio = dto.GastosEnvio,
                EsElectronica = dto.EsElectronica,
                EsPresupuesto = dto.EsPresupuesto
            };

            var detalles = dto.Detalles.Select(d => new ComprobanteDetalle
            {
                Articulo_Id = d.Articulo_Id,
                Cantidad = d.Cantidad,
                Precio_Unitario = d.Precio_Unitario,
                Subtotal = d.Subtotal
            }).ToList();

            var updatedComprobante = await _comprobanteRepository.UpdateAsync(id, comprobante, detalles);

            if (updatedComprobante == null)
                return null;

            return await _comprobanteRepository.GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _comprobanteRepository.DeleteAsync(id);
        }

        public async Task<ComprobanteConDetallesDto> CancelarAsync(int comprobanteId)
        {
            // Obtener el comprobante original
            var comprobanteOriginal = await _comprobanteRepository.GetByIdAsync(comprobanteId);
            if (comprobanteOriginal == null)
            {
                throw new Exception($"Comprobante con ID {comprobanteId} no encontrado");
            }

            // Verificar que sea una factura electrónica (FC)
            if (comprobanteOriginal.TipoComprobante != "FC")
            {
                throw new Exception("Solo se pueden cancelar facturas electrónicas (FC)");
            }

            // Verificar que no haya sido cancelado previamente
            if (comprobanteOriginal.EstaCancelado)
            {
                throw new Exception($"Esta factura ya fue cancelada por la Nota de Crédito {comprobanteOriginal.NotaCreditoNumero}");
            }

            // Crear el comprobante de Nota de Crédito con los mismos datos
            var notaCredito = new Comprobante
            {
                Cliente_Id = comprobanteOriginal.Cliente_Id,
                Fecha = DateTime.Now,
                TipoComprobante = "NC",
                Total = comprobanteOriginal.Total,
                Bonificacion = comprobanteOriginal.Bonificacion,
                PorcentajeBonif = comprobanteOriginal.PorcentajeBonif,
                Anticipo = comprobanteOriginal.Anticipo,
                ContraEntrega = comprobanteOriginal.ContraEntrega,
                Cuotas = comprobanteOriginal.Cuotas,
                ValorCuota = comprobanteOriginal.ValorCuota,
                Vendedor_Id = comprobanteOriginal.Vendedor_Id,
                GastosEnvio = comprobanteOriginal.GastosEnvio,
                EsElectronica = true,
                EsPresupuesto = false,
                ComprobanteAsociado_Id = comprobanteId  // Relación con la factura original
            };

            // Obtener los detalles del comprobante original
            var detallesOriginales = await _comprobanteRepository.GetDetallesByComprobanteIdAsync(comprobanteId);
            var detalles = detallesOriginales.Select(d => new ComprobanteDetalle
            {
                Articulo_Id = d.Articulo_Id,
                Cantidad = d.Cantidad,
                Precio_Unitario = d.Precio_Unitario,
                Subtotal = d.Subtotal
            }).ToList();

            // Extraer punto de venta y número del comprobante original
            // Formato: "XXXXX-XXXXXXXX" (5 dígitos punto venta - 8 dígitos número)
            int puntoVentaOriginal = 0;
            long numeroOriginal = 0;
            if (!string.IsNullOrEmpty(comprobanteOriginal.NumeroComprobante))
            {
                var partes = comprobanteOriginal.NumeroComprobante.Split('-');
                if (partes.Length == 2)
                {
                    int.TryParse(partes[0], out puntoVentaOriginal);
                    long.TryParse(partes[1], out numeroOriginal);
                }
            }

            // Crear información del comprobante asociado
            var comprobanteAsociado = new ComprobanteAsociadoInfo
            {
                Tipo = 11, // Factura C
                PuntoVenta = puntoVentaOriginal,
                Numero = numeroOriginal,
                Fecha = comprobanteOriginal.Fecha
            };

            // Solicitar CAE para Nota de Crédito C (tipo 13)
            try
            {
                _logger.LogInformation("Solicitando CAE para Nota de Crédito del comprobante {Id}", comprobanteId);
                var caeResponse = await _afipService.SolicitarCAEAsync(notaCredito, detalles, 13, comprobanteAsociado);

                if (caeResponse.Success)
                {
                    notaCredito.CAE = caeResponse.CAE;
                    notaCredito.VTO = caeResponse.CAEVencimiento;
                    notaCredito.NumeroComprobante = caeResponse.NumeroComprobante;
                    _logger.LogInformation("CAE para NC obtenido exitosamente: {CAE}", caeResponse.CAE);
                }
                else
                {
                    var errores = string.Join(", ", caeResponse.Errores);
                    _logger.LogError("Error al obtener CAE para NC: {Errores}", errores);
                    throw new Exception($"Error al obtener CAE para Nota de Crédito: {errores}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al solicitar CAE para Nota de Crédito");
                throw new Exception($"Error al solicitar CAE para Nota de Crédito: {ex.Message}", ex);
            }

            // Crear la Nota de Crédito en la base de datos
            var notaCreditoCreada = await _comprobanteRepository.CreateAsync(notaCredito, detalles);

            // Eliminar las cuotas del comprobante original (ya no aplican)
            _logger.LogInformation("Eliminando cuotas del comprobante cancelado {Id}", comprobanteId);
            await _cuotaRepository.DeleteByComprobanteIdAsync(comprobanteId);

            return await _comprobanteRepository.GetByIdAsync(notaCreditoCreada.Id)
                ?? throw new Exception("Error al recuperar la Nota de Crédito creada");
        }
    }
}
