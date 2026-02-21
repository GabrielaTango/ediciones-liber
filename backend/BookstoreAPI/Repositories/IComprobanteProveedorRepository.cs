using BookstoreAPI.DTOs;
using BookstoreAPI.Models;

namespace BookstoreAPI.Repositories
{
    public interface IComprobanteProveedorRepository
    {
        Task<IEnumerable<ComprobanteProveedorListDto>> GetAllAsync();
        Task<ComprobanteProveedorListDto?> GetByIdAsync(int id);
        Task<ComprobanteProveedorDetailDto?> GetDetailByIdAsync(int id);
        Task<ComprobanteProveedor> CreateAsync(ComprobanteProveedor comprobante, List<CuotaProveedor> cuotas);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<IvaComprasDto>> GetIvaComprasAsync(DateTime fechaDesde, DateTime fechaHasta);
    }
}
