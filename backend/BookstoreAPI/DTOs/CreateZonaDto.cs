using System.ComponentModel.DataAnnotations;

namespace BookstoreAPI.DTOs
{
    public class CreateZonaDto
    {
        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres")]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class UpdateZonaDto
    {
        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
