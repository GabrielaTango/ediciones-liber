namespace BookstoreAPI.Models
{
    public class Cuota
    {
        public int Id { get; set; }
        public int? Comprobante_Id { get; set; }
        public int NumeroCuota { get; set; } // 0 = contraentrega, 1+ = cuotas regulares
        public DateTime? Fecha { get; set; }
        public decimal? Importe { get; set; }
        public decimal? ImportePagado { get; set; }
        public string? Estado { get; set; }
    }
}
