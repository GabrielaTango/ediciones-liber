namespace BookstoreAPI.Models
{
    public class PagoCuota
    {
        public int Id { get; set; }
        public int Cuota_Id { get; set; }
        public string NroReferencia { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
    }
}
