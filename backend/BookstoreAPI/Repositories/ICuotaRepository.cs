using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using System.Data;

namespace BookstoreAPI.Repositories
{
    public interface ICuotaRepository
    {
        Task<IEnumerable<Cuota>> GetByComprobanteIdAsync(int comprobanteId);
        Task CreateCuotasAsync(int comprobanteId, List<Cuota> cuotas, IDbConnection connection, IDbTransaction transaction);
        Task DeleteByComprobanteIdAsync(int comprobanteId, IDbConnection connection, IDbTransaction transaction);
        Task DeleteByComprobanteIdAsync(int comprobanteId);
        Task<IEnumerable<CuotaListadoDto>> GetCuotasByFiltrosAsync(int? zonaId, DateTime? fechaCorte, int? vendedorId = null, string? comprobante = null, int? clienteId = null);
        Task<bool> UpdateImportePagadoAsync(int cuotaId, decimal importePagado);
        Task DeletePendientesByComprobanteIdAsync(int comprobanteId);
        Task<PagoCuota> CreatePagoAsync(int cuotaId, PagoCuota pago);
        Task<bool> DeletePagoAsync(int pagoId);
        Task CreatePagoComprobanteAsync(int comprobanteId, string nroReferencia, decimal importe, DateTime? fecha = null);
        Task CancelarByComprobanteIdAsync(int comprobanteId);
    }
}
