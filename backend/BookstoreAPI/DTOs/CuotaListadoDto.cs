namespace BookstoreAPI.DTOs
{
    public class CuotaListadoDto
    {
        public int Id { get; set; }
        public int ComprobanteId { get; set; }
        public string? NumeroComprobante { get; set; }
        public DateTime? FechaComprobante { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public int? ZonaId { get; set; }
        public string? ZonaNombre { get; set; }
        public DateTime? FechaCuota { get; set; }
        public decimal Importe { get; set; }
        public decimal ImportePagado { get; set; }
        public string? Estado { get; set; }
        public int NumeroCuota { get; set; } // 0 = contraentrega, 1+ = cuotas regulares
        public bool EsCuotaCero => NumeroCuota == 0; // Calculado para compatibilidad
        public List<PagoCuotaDto> Pagos { get; set; } = new();
    }

    public class PagoCuotaDto
    {
        public int Id { get; set; }
        public string NroReferencia { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
    }

    public class CreatePagoCuotaDto
    {
        public string NroReferencia { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
    }

    public class CreatePagoComprobanteDto
    {
        public string NroReferencia { get; set; } = string.Empty;
        public decimal Importe { get; set; }
        public DateTime? Fecha { get; set; }
    }
}
