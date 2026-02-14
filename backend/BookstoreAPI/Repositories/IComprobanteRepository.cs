using BookstoreAPI.DTOs;
using BookstoreAPI.Models;

namespace BookstoreAPI.Repositories
{
    public interface IComprobanteRepository
    {
        Task<IEnumerable<ComprobanteConDetallesDto>> GetAllAsync();
        Task<IEnumerable<ComprobanteConDetallesDto>> GetAllFilteredAsync(int? zonaId, int? clienteId, string? tipoComprobante, DateTime? fechaDesde, DateTime? fechaHasta, int? vendedorId = null);
        Task<ComprobanteConDetallesDto?> GetByIdAsync(int id);
        Task<Comprobante?> GetComprobanteByIdAsync(int id);
        Task<Comprobante> CreateAsync(Comprobante comprobante, List<ComprobanteDetalle> detalles);
        Task<Comprobante?> UpdateAsync(int id, Comprobante comprobante, List<ComprobanteDetalle> detalles);
        Task<bool> DeleteAsync(int id);
        Task<List<ComprobanteDetalle>> GetDetallesByComprobanteIdAsync(int comprobanteId);
        Task<IEnumerable<IvaVentasDto>> GetIvaVentasAsync(DateTime fechaDesde, DateTime fechaHasta);
        Task<DeudoresReporteDto> GetDeudoresAsync(int mes, int anio, int? zonaId = null);
        Task<string> GetSiguienteNumeroPresupuestoAsync(string puntoVenta);
        Task<ArticulosVendidosZonaReporteDto> GetArticulosVendidosPorZonaAsync(int? zonaId);
        Task UpdateEstadoAsync(int comprobanteId, string estado);
        Task<decimal> GetUltimoGastoEnvioAsync();
    }
}
