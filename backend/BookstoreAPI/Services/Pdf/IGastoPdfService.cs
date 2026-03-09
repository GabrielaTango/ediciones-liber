using BookstoreAPI.Models;

namespace BookstoreAPI.Services.Pdf
{
    public interface IGastoPdfService
    {
        byte[] GenerarPdf(List<Gasto> gastos, DateTime fechaDesde, DateTime fechaHasta);
    }
}
