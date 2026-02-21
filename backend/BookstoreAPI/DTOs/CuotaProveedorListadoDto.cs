namespace BookstoreAPI.DTOs
{
    public class CuotaProveedorListadoDto
    {
        public int Id { get; set; }
        public int ComprobanteProveedorId { get; set; }
        public string NroComprobante { get; set; } = string.Empty;
        public string TipoComprobante { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public int ProveedorId { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;
        public int NumeroCuota { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal Importe { get; set; }
        public decimal ImportePagado { get; set; }
        public string Estado { get; set; } = string.Empty;
        public List<PagoCuotaProveedorDto> Pagos { get; set; } = new();
    }

    public class PagoCuotaProveedorDto
    {
        public int Id { get; set; }
        public string NroReferencia { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
    }

    public class CreatePagoCuotaProveedorDto
    {
        public string NroReferencia { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
    }
}
