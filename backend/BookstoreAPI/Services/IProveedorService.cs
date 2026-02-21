using BookstoreAPI.DTOs;
using BookstoreAPI.Models;

namespace BookstoreAPI.Services
{
    public interface IProveedorService
    {
        Task<IEnumerable<Proveedor>> GetAllProveedoresAsync();
        Task<Proveedor?> GetProveedorByIdAsync(int id);
        Task<Proveedor> CreateProveedorAsync(CreateProveedorDto createDto);
        Task<Proveedor?> UpdateProveedorAsync(int id, UpdateProveedorDto updateDto);
        Task<bool> DeleteProveedorAsync(int id);
    }
}
