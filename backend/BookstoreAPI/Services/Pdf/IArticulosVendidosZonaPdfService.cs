using BookstoreAPI.DTOs;

namespace BookstoreAPI.Services.Pdf
{
    public interface IArticulosVendidosZonaPdfService
    {
        byte[] GenerarPdf(ArticulosVendidosZonaReporteDto reporte);
    }
}
