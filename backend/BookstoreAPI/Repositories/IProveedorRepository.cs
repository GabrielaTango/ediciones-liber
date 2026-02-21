using BookstoreAPI.Models;

namespace BookstoreAPI.Repositories
{
    public interface IProveedorRepository
    {
        Task<IEnumerable<Proveedor>> GetAllAsync();
        Task<Proveedor?> GetByIdAsync(int id);
        Task<int> CreateAsync(Proveedor proveedor);
        Task<bool> UpdateAsync(int id, Proveedor proveedor);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetNextCodigoAsync();
    }
}
