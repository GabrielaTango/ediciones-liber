using BookstoreAPI.Models;

namespace BookstoreAPI.Repositories
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByUsernameAsync(string username);
        Task<int> CreateAsync(Usuario usuario);
        Task<bool> UpdateAsync(int id, Usuario usuario);
        Task<bool> UpdatePasswordAsync(int id, string passwordHash);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateUltimoAccesoAsync(int id);
    }
}
