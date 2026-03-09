namespace BookstoreAPI.Models
{
    public class SubZona
    {
        public int Id { get; set; }
        public string? Descripcion { get; set; }
        public int ProvinciaId { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Localidad { get; set; }
        public string? ProvinciaDescripcion { get; set; }
    }
}
