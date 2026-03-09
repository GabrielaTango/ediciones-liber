using System.ComponentModel.DataAnnotations;

namespace BookstoreAPI.DTOs
{
    public class CreateVendedorDto
    {
        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres")]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class UpdateVendedorDto
    {
        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
