using BookstoreAPI.DTOs;

namespace BookstoreAPI.Services
{
    public interface IComprobanteProveedorService
    {
        Task<IEnumerable<ComprobanteProveedorListDto>> GetAllAsync();
        Task<ComprobanteProveedorListDto?> GetByIdAsync(int id);
        Task<ComprobanteProveedorDetailDto?> GetDetailByIdAsync(int id);
        Task<ComprobanteProveedorListDto> CreateAsync(CreateComprobanteProveedorDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
