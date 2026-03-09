using System.ComponentModel.DataAnnotations;

namespace BookstoreAPI.DTOs
{
    public class CreateTransporteDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string? Direccion { get; set; }

        [MaxLength(100, ErrorMessage = "La localidad no puede exceder los 100 caracteres")]
        public string? Localidad { get; set; }

        public int? ProvinciaId { get; set; }

        [MaxLength(13, ErrorMessage = "El CUIT no puede exceder los 13 caracteres")]
        public string? Cuit { get; set; }
    }

    public class UpdateTransporteDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string? Direccion { get; set; }

        [MaxLength(100, ErrorMessage = "La localidad no puede exceder los 100 caracteres")]
        public string? Localidad { get; set; }

        public int? ProvinciaId { get; set; }

        [MaxLength(13, ErrorMessage = "El CUIT no puede exceder los 13 caracteres")]
        public string? Cuit { get; set; }
    }
}
