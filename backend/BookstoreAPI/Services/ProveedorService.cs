using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using BookstoreAPI.Repositories;

namespace BookstoreAPI.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly IProveedorRepository _proveedorRepository;

        public ProveedorService(IProveedorRepository proveedorRepository)
        {
            _proveedorRepository = proveedorRepository;
        }

        public async Task<IEnumerable<Proveedor>> GetAllProveedoresAsync()
        {
            return await _proveedorRepository.GetAllAsync();
        }

        public async Task<Proveedor?> GetProveedorByIdAsync(int id)
        {
            return await _proveedorRepository.GetByIdAsync(id);
        }

        public async Task<Proveedor> CreateProveedorAsync(CreateProveedorDto createDto)
        {
            var nextCodigo = await _proveedorRepository.GetNextCodigoAsync();

            var proveedor = new Proveedor
            {
                Codigo = nextCodigo.ToString(),
                Nombre = createDto.Nombre,
                RazonSocial = createDto.RazonSocial,
                Cuit = createDto.Cuit,
                Domicilio = createDto.Domicilio,
                Telefono = createDto.Telefono,
                Mail = createDto.Mail,
            };

            var id = await _proveedorRepository.CreateAsync(proveedor);
            proveedor.Id = id;
            return proveedor;
        }

        public async Task<Proveedor?> UpdateProveedorAsync(int id, UpdateProveedorDto updateDto)
        {
            var existingProveedor = await _proveedorRepository.GetByIdAsync(id);
            if (existingProveedor == null)
            {
                return null;
            }

            existingProveedor.Nombre = updateDto.Nombre;
            existingProveedor.RazonSocial = updateDto.RazonSocial;
            existingProveedor.Cuit = updateDto.Cuit;
            existingProveedor.Domicilio = updateDto.Domicilio;
            existingProveedor.Telefono = updateDto.Telefono;
            existingProveedor.Mail = updateDto.Mail;
            existingProveedor.FechaInhabilitacion = updateDto.FechaInhabilitacion;

            var updated = await _proveedorRepository.UpdateAsync(id, existingProveedor);
            return updated ? existingProveedor : null;
        }

        public async Task<bool> DeleteProveedorAsync(int id)
        {
            var exists = await _proveedorRepository.ExistsAsync(id);
            if (!exists)
            {
                return false;
            }

            return await _proveedorRepository.DeleteAsync(id);
        }
    }
}
