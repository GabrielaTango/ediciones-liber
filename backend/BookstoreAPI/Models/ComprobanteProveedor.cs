namespace BookstoreAPI.Models
{
    public class ComprobanteProveedor
    {
        public int Id { get; set; }
        public int Proveedor_Id { get; set; }
        public string TipoComprobante { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string NroComprobante { get; set; } = string.Empty;
        public decimal ImporteTotal { get; set; }
        public int CantidadCuotas { get; set; }
        public DateTime FechaPrimerVencimiento { get; set; }
    }
}
