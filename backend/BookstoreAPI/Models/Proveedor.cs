namespace BookstoreAPI.Models
{
    public class Proveedor
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? RazonSocial { get; set; }
        public string? Cuit { get; set; }
        public string? Domicilio { get; set; }
        public string? Telefono { get; set; }
        public string? Mail { get; set; }
        public DateTime? FechaInhabilitacion { get; set; }
    }
}
