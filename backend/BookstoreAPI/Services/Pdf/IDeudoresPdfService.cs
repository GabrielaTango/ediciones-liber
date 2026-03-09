using BookstoreAPI.DTOs;

namespace BookstoreAPI.Services.Pdf
{
    public interface IDeudoresPdfService
    {
        byte[] GenerarPdf(DeudoresReporteDto reporte);
    }
}
