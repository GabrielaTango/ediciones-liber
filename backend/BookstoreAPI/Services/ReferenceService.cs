using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using BookstoreAPI.Repositories;

namespace BookstoreAPI.Services
{
    public class ReferenceService : IReferenceService
    {
        private readonly IReferenceRepository _referenceRepository;

        public ReferenceService(IReferenceRepository referenceRepository)
        {
            _referenceRepository = referenceRepository;
        }

        // Zona - CRUD Operations
        public async Task<IEnumerable<Zona>> GetAllZonasAsync()
        {
            return await _referenceRepository.GetAllZonasAsync();
        }

        public async Task<Zona?> GetZonaByIdAsync(int id)
        {
            return await _referenceRepository.GetZonaByIdAsync(id);
        }

        public async Task<Zona> CreateZonaAsync(CreateZonaDto dto)
        {
            var zona = new Zona
            {
                Descripcion = dto.Descripcion
            };
            return await _referenceRepository.CreateZonaAsync(zona);
        }

        public async Task<Zona?> UpdateZonaAsync(int id, UpdateZonaDto dto)
        {
            var zona = new Zona
            {
                Descripcion = dto.Descripcion
            };
            return await _referenceRepository.UpdateZonaAsync(id, zona);
        }

        public async Task<bool> DeleteZonaAsync(int id)
        {
            return await _referenceRepository.DeleteZonaAsync(id);
        }

        // SubZona - CRUD Operations
        public async Task<IEnumerable<SubZona>> GetAllSubZonasAsync()
        {
            return await _referenceRepository.GetAllSubZonasAsync();
        }

        public async Task<IEnumerable<SubZona>> GetSubZonasByZonaIdAsync(int zonaId)
        {
            return await _referenceRepository.GetSubZonasByZonaIdAsync(zonaId);
        }

        public async Task<SubZona?> GetSubZonaByIdAsync(int id)
        {
            return await _referenceRepository.GetSubZonaByIdAsync(id);
        }

        public async Task<SubZona> CreateSubZonaAsync(CreateSubZonaDto dto)
        {
            var subZona = new SubZona
            {
                Descripcion = dto.Descripcion,
                ZonaId = dto.ZonaId,
                ProvinciaId = dto.ProvinciaId,
                CodigoPostal = dto.CodigoPostal,
                Localidad = dto.Localidad
            };
            return await _referenceRepository.CreateSubZonaAsync(subZona);
        }

        public async Task<SubZona?> UpdateSubZonaAsync(int id, UpdateSubZonaDto dto)
        {
            var subZona = new SubZona
            {
                Descripcion = dto.Descripcion,
                ZonaId = dto.ZonaId,
                ProvinciaId = dto.ProvinciaId,
                CodigoPostal = dto.CodigoPostal,
                Localidad = dto.Localidad
            };
            return await _referenceRepository.UpdateSubZonaAsync(id, subZona);
        }

        public async Task<bool> DeleteSubZonaAsync(int id)
        {
            return await _referenceRepository.DeleteSubZonaAsync(id);
        }

        // Provincia - CRUD Operations
        public async Task<IEnumerable<Provincia>> GetAllProvinciasAsync()
        {
            return await _referenceRepository.GetAllProvinciasAsync();
        }

        public async Task<Provincia?> GetProvinciaByIdAsync(int id)
        {
            return await _referenceRepository.GetProvinciaByIdAsync(id);
        }

        public async Task<Provincia> CreateProvinciaAsync(CreateProvinciaDto dto)
        {
            var provincia = new Provincia
            {
                Descripcion = dto.Descripcion
            };
            return await _referenceRepository.CreateProvinciaAsync(provincia);
        }

        public async Task<Provincia?> UpdateProvinciaAsync(int id, UpdateProvinciaDto dto)
        {
            var provincia = new Provincia
            {
                Descripcion = dto.Descripcion
            };
            return await _referenceRepository.UpdateProvinciaAsync(id, provincia);
        }

        public async Task<bool> DeleteProvinciaAsync(int id)
        {
            return await _referenceRepository.DeleteProvinciaAsync(id);
        }

        // Vendedor - CRUD Operations
        public async Task<IEnumerable<Vendedor>> GetAllVendedoresAsync()
        {
            return await _referenceRepository.GetAllVendedoresAsync();
        }

        public async Task<Vendedor?> GetVendedorByIdAsync(int id)
        {
            return await _referenceRepository.GetVendedorByIdAsync(id);
        }

        public async Task<Vendedor> CreateVendedorAsync(CreateVendedorDto dto)
        {
            var vendedor = new Vendedor
            {
                Descripcion = dto.Descripcion
            };
            return await _referenceRepository.CreateVendedorAsync(vendedor);
        }

        public async Task<Vendedor?> UpdateVendedorAsync(int id, UpdateVendedorDto dto)
        {
            var vendedor = new Vendedor
            {
                Descripcion = dto.Descripcion
            };
            return await _referenceRepository.UpdateVendedorAsync(id, vendedor);
        }

        public async Task<bool> DeleteVendedorAsync(int id)
        {
            return await _referenceRepository.DeleteVendedorAsync(id);
        }

        // Transporte - CRUD Operations
        public async Task<IEnumerable<Transporte>> GetAllTransportesAsync()
        {
            return await _referenceRepository.GetAllTransportesAsync();
        }

        public async Task<Transporte?> GetTransporteByIdAsync(int id)
        {
            return await _referenceRepository.GetTransporteByIdAsync(id);
        }

        public async Task<Transporte> CreateTransporteAsync(CreateTransporteDto dto)
        {
            var transporte = new Transporte
            {
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                Localidad = dto.Localidad,
                ProvinciaId = dto.ProvinciaId,
                Cuit = dto.Cuit
            };
            return await _referenceRepository.CreateTransporteAsync(transporte);
        }

        public async Task<Transporte?> UpdateTransporteAsync(int id, UpdateTransporteDto dto)
        {
            var transporte = new Transporte
            {
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                Localidad = dto.Localidad,
                ProvinciaId = dto.ProvinciaId,
                Cuit = dto.Cuit
            };
            return await _referenceRepository.UpdateTransporteAsync(id, transporte);
        }

        public async Task<bool> DeleteTransporteAsync(int id)
        {
            return await _referenceRepository.DeleteTransporteAsync(id);
        }
    }
}
