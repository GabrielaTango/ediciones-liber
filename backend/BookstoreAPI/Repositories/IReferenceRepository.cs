using BookstoreAPI.Models;

namespace BookstoreAPI.Repositories
{
    public interface IReferenceRepository
    {
        // Zona
        Task<IEnumerable<Zona>> GetAllZonasAsync();
        Task<Zona?> GetZonaByIdAsync(int id);
        Task<Zona> CreateZonaAsync(Zona zona);
        Task<Zona?> UpdateZonaAsync(int id, Zona zona);
        Task<bool> DeleteZonaAsync(int id);

        // SubZona
        Task<IEnumerable<SubZona>> GetAllSubZonasAsync();
        Task<IEnumerable<SubZona>> GetSubZonasByZonaIdAsync(int zonaId);
        Task<SubZona?> GetSubZonaByIdAsync(int id);
        Task<SubZona> CreateSubZonaAsync(SubZona subZona);
        Task<SubZona?> UpdateSubZonaAsync(int id, SubZona subZona);
        Task<bool> DeleteSubZonaAsync(int id);

        // Provincia
        Task<IEnumerable<Provincia>> GetAllProvinciasAsync();
        Task<Provincia?> GetProvinciaByIdAsync(int id);
        Task<Provincia> CreateProvinciaAsync(Provincia provincia);
        Task<Provincia?> UpdateProvinciaAsync(int id, Provincia provincia);
        Task<bool> DeleteProvinciaAsync(int id);

        // Vendedor
        Task<IEnumerable<Vendedor>> GetAllVendedoresAsync();
        Task<Vendedor?> GetVendedorByIdAsync(int id);
        Task<Vendedor> CreateVendedorAsync(Vendedor vendedor);
        Task<Vendedor?> UpdateVendedorAsync(int id, Vendedor vendedor);
        Task<bool> DeleteVendedorAsync(int id);

        // Transporte
        Task<IEnumerable<Transporte>> GetAllTransportesAsync();
        Task<Transporte?> GetTransporteByIdAsync(int id);
        Task<Transporte> CreateTransporteAsync(Transporte transporte);
        Task<Transporte?> UpdateTransporteAsync(int id, Transporte transporte);
        Task<bool> DeleteTransporteAsync(int id);
    }
}
