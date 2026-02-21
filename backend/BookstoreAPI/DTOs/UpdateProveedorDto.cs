using System.ComponentModel.DataAnnotations;

namespace BookstoreAPI.DTOs
{
    public class UpdateProveedorDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La razón social no puede exceder 200 caracteres")]
        public string? RazonSocial { get; set; }

        [StringLength(20, ErrorMessage = "El CUIT no puede exceder 20 caracteres")]
        public string? Cuit { get; set; }

        [StringLength(300, ErrorMessage = "El domicilio no puede exceder 300 caracteres")]
        public string? Domicilio { get; set; }

        [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no es válido")]
        [StringLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
        public string? Mail { get; set; }

        public DateTime? FechaInhabilitacion { get; set; }
    }
}
