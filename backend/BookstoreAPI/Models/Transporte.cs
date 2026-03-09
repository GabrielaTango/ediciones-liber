namespace BookstoreAPI.Models
{
    public class Transporte
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public int? ProvinciaId { get; set; }
        public string? ProvinciaDescripcion { get; set; }
        public string? Cuit { get; set; }
    }
}
