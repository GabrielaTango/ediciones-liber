namespace BookstoreAPI.DTOs
{
    public class IvaComprasDto
    {
        public DateTime Fecha { get; set; }
        public string TipoComprobante { get; set; } = string.Empty;
        public string? NumeroComprobante { get; set; }
        public string? Nombre { get; set; }
        public string? Cuit { get; set; }
        public decimal Total { get; set; }
    }
}
