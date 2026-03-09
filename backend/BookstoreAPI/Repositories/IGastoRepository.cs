using BookstoreAPI.Models;

namespace BookstoreAPI.Repositories
{
    public interface IGastoRepository
    {
        Task<IEnumerable<Gasto>> GetAllAsync();
        Task<Gasto?> GetByIdAsync(int id);
        Task<int> CreateAsync(Gasto gasto);
        Task<bool> UpdateAsync(int id, Gasto gasto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<string>> GetCategoriasAsync();
        Task<IEnumerable<Gasto>> GetByFechaRangoAsync(DateTime fechaDesde, DateTime fechaHasta);
    }
}
