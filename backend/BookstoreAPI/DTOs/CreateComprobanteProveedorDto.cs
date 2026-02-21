namespace BookstoreAPI.DTOs
{
    public class CreateComprobanteProveedorDto
    {
        public int Proveedor_Id { get; set; }
        public string TipoComprobante { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string NroComprobante { get; set; } = string.Empty;
        public decimal ImporteTotal { get; set; }
        public int CantidadCuotas { get; set; }
        public DateTime FechaPrimerVencimiento { get; set; }
    }

    public class ComprobanteProveedorListDto
    {
        public int Id { get; set; }
        public int Proveedor_Id { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;
        public string TipoComprobante { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string NroComprobante { get; set; } = string.Empty;
        public decimal ImporteTotal { get; set; }
        public int CantidadCuotas { get; set; }
        public DateTime FechaPrimerVencimiento { get; set; }
    }

    public class ComprobanteProveedorDetailDto
    {
        public int Id { get; set; }
        public int Proveedor_Id { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;
        public string TipoComprobante { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string NroComprobante { get; set; } = string.Empty;
        public decimal ImporteTotal { get; set; }
        public int CantidadCuotas { get; set; }
        public DateTime FechaPrimerVencimiento { get; set; }
        public List<CuotaProveedorDto> Cuotas { get; set; } = new();
    }

    public class CuotaProveedorDto
    {
        public int Id { get; set; }
        public int NumeroCuota { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal Importe { get; set; }
        public decimal ImportePagado { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
