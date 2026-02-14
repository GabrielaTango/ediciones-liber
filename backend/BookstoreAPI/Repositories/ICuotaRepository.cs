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
        Task<IEnumerable<CuotaListadoDto>> GetCuotasByFiltrosAsync(int? zonaId, int? mes, int? anio);
        Task<bool> UpdateImportePagadoAsync(int cuotaId, decimal importePagado);
        Task DeletePendientesByComprobanteIdAsync(int comprobanteId);
    }
}
