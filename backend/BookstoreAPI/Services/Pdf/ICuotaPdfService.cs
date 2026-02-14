using BookstoreAPI.Models;

namespace BookstoreAPI.Services.Pdf
{
    public interface ICuotaPdfService
    {
        byte[] GenerarCuponesPdf(Comprobante comprobante, Cliente cliente, List<Cuota> cuotas);
        byte[] GenerarLoteCuponesPdf(List<(Comprobante comprobante, Cliente cliente, List<Cuota> cuotas)> lote);
    }
}
