using BookstoreAPI.DTOs;
using BookstoreAPI.Models;

namespace BookstoreAPI.Services
{
    public interface IGastoService
    {
        Task<IEnumerable<Gasto>> GetAllGastosAsync();
        Task<Gasto?> GetGastoByIdAsync(int id);
        Task<Gasto> CreateGastoAsync(CreateGastoDto createDto);
        Task<Gasto?> UpdateGastoAsync(int id, UpdateGastoDto updateDto);
        Task<bool> DeleteGastoAsync(int id);
        Task<IEnumerable<string>> GetCategoriasAsync();
        Task<IEnumerable<Gasto>> GetGastosByFechaRangoAsync(DateTime fechaDesde, DateTime fechaHasta);
    }
}
