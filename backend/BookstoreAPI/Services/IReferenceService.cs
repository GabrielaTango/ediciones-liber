using BookstoreAPI.DTOs;
using BookstoreAPI.Models;

namespace BookstoreAPI.Services
{
    public interface IReferenceService
    {
        // Zona
        Task<IEnumerable<Zona>> GetAllZonasAsync();
        Task<Zona?> GetZonaByIdAsync(int id);
        Task<Zona> CreateZonaAsync(CreateZonaDto dto);
        Task<Zona?> UpdateZonaAsync(int id, UpdateZonaDto dto);
        Task<bool> DeleteZonaAsync(int id);

        // SubZona
        Task<IEnumerable<SubZona>> GetAllSubZonasAsync();
        Task<IEnumerable<SubZona>> GetSubZonasByZonaIdAsync(int zonaId);
        Task<SubZona?> GetSubZonaByIdAsync(int id);
        Task<SubZona> CreateSubZonaAsync(CreateSubZonaDto dto);
        Task<SubZona?> UpdateSubZonaAsync(int id, UpdateSubZonaDto dto);
        Task<bool> DeleteSubZonaAsync(int id);

        // Provincia
        Task<IEnumerable<Provincia>> GetAllProvinciasAsync();
        Task<Provincia?> GetProvinciaByIdAsync(int id);
        Task<Provincia> CreateProvinciaAsync(CreateProvinciaDto dto);
        Task<Provincia?> UpdateProvinciaAsync(int id, UpdateProvinciaDto dto);
        Task<bool> DeleteProvinciaAsync(int id);

        // Vendedor
        Task<IEnumerable<Vendedor>> GetAllVendedoresAsync();
        Task<Vendedor?> GetVendedorByIdAsync(int id);
        Task<Vendedor> CreateVendedorAsync(CreateVendedorDto dto);
        Task<Vendedor?> UpdateVendedorAsync(int id, UpdateVendedorDto dto);
        Task<bool> DeleteVendedorAsync(int id);

        // Transporte
        Task<IEnumerable<Transporte>> GetAllTransportesAsync();
        Task<Transporte?> GetTransporteByIdAsync(int id);
        Task<Transporte> CreateTransporteAsync(CreateTransporteDto dto);
        Task<Transporte?> UpdateTransporteAsync(int id, UpdateTransporteDto dto);
        Task<bool> DeleteTransporteAsync(int id);
    }
}
