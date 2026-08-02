using BookstoreAPI.DTOs;

namespace BookstoreAPI.Services.Afip
{
    public interface IAfipCertificadoService
    {
        Task<CertificadoEstadoDto> GetEstadoAsync();
    }
}
