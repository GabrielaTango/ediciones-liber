namespace BookstoreAPI.Models
{
    public class CuotaProveedor
    {
        public int Id { get; set; }
        public int ComprobanteProveedor_Id { get; set; }
        public int NumeroCuota { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal Importe { get; set; }
        public decimal ImportePagado { get; set; }
        public string Estado { get; set; } = "PEN";
    }
}
