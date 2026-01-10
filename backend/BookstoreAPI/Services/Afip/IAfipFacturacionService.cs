using BookstoreAPI.Models;
using BookstoreAPI.Models.Afip;

namespace BookstoreAPI.Services.Afip
{
    /// <summary>
    /// Información del comprobante asociado para Notas de Crédito/Débito
    /// </summary>
    public class ComprobanteAsociadoInfo
    {
        public int Tipo { get; set; }  // Tipo de comprobante AFIP (ej: 11 para Factura C)
        public int PuntoVenta { get; set; }
        public long Numero { get; set; }
        public DateTime Fecha { get; set; }
    }

    public interface IAfipFacturacionService
    {
        Task<AfipCAEResponse> SolicitarCAEAsync(Comprobante comprobante, List<ComprobanteDetalle> detalles);
        Task<AfipCAEResponse> SolicitarCAEAsync(Comprobante comprobante, List<ComprobanteDetalle> detalles, int tipoComprobanteAfip);
        Task<AfipCAEResponse> SolicitarCAEAsync(Comprobante comprobante, List<ComprobanteDetalle> detalles, int tipoComprobanteAfip, ComprobanteAsociadoInfo? comprobanteAsociado);
        Task<int> GetUltimoComprobanteAutorizadoAsync(int puntoVenta, int tipoComprobante);
    }
}
