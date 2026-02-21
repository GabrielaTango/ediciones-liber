using BookstoreAPI.DTOs;
using BookstoreAPI.Models;

namespace BookstoreAPI.Repositories
{
    public interface ICuotaProveedorRepository
    {
        Task<IEnumerable<CuotaProveedorListadoDto>> GetCuotasByFiltrosAsync(int? proveedorId, DateTime? fechaDesde, DateTime? fechaHasta, string? estado);
        Task<PagoCuotaProveedor> CreatePagoAsync(int cuotaId, PagoCuotaProveedor pago);
        Task<bool> DeletePagoAsync(int pagoId);
    }
}
