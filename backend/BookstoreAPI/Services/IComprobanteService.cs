using BookstoreAPI.DTOs;

namespace BookstoreAPI.Services
{
    public interface IComprobanteService
    {
        Task<IEnumerable<ComprobanteConDetallesDto>> GetAllAsync();
        Task<ComprobanteConDetallesDto?> GetByIdAsync(int id);
        Task<ComprobanteConDetallesDto> CreateAsync(CreateComprobanteDto dto);
        Task<ComprobanteConDetallesDto?> UpdateAsync(int id, UpdateComprobanteDto dto);
        Task<bool> DeleteAsync(int id);
        Task<ComprobanteConDetallesDto> CancelarAsync(int comprobanteId);
    }
}
