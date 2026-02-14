using BookstoreAPI.Models;

namespace BookstoreAPI.Services.Pdf
{
    public interface IComprobantePdfService
    {
        /// <summary>
        /// Genera el PDF del comprobante con el QR de AFIP
        /// </summary>
        byte[] GenerarPdf(Comprobante comprobante, Cliente cliente, List<ComprobanteDetalle> detalles);

        /// <summary>
        /// Genera el PDF completo: comprobante x3 (triplicado) + cupones de cuotas
        /// </summary>
        byte[] GenerarComprobanteCompletoConCupones(Comprobante comprobante, Cliente cliente, List<ComprobanteDetalle> detalles, List<Cuota> cuotas);

        /// <summary>
        /// Genera un PDF con múltiples comprobantes por triplicado (sin cupones)
        /// </summary>
        byte[] GenerarLotePdf(List<(Comprobante comprobante, Cliente cliente, List<ComprobanteDetalle> detalles)> lote);
    }
}
