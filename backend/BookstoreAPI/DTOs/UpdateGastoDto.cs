using System.ComponentModel.DataAnnotations;

namespace BookstoreAPI.DTOs
{
    public class UpdateGastoDto
    {
        [Required(ErrorMessage = "El número de comprobante es requerido")]
        [StringLength(50, ErrorMessage = "El número de comprobante no puede exceder 50 caracteres")]
        public string NroComprobante { get; set; } = string.Empty;

        [Required(ErrorMessage = "El importe es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El importe debe ser mayor a 0")]
        public decimal Importe { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        [StringLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres")]
        public string Categoria { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; }
    }
}
