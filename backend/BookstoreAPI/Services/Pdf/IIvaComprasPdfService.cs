using BookstoreAPI.DTOs;

namespace BookstoreAPI.Services.Pdf
{
    public interface IIvaComprasPdfService
    {
        byte[] GenerarPdf(List<IvaComprasDto> compras, DateTime fechaDesde, DateTime fechaHasta);
    }
}
