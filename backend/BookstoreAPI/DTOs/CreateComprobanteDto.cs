using System.ComponentModel.DataAnnotations;

namespace BookstoreAPI.DTOs
{
    public class CreateComprobanteDto
    {
        [Required(ErrorMessage = "El cliente es obligatorio")]
        public int Cliente_Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Fecha { get; set; }

        public string? TipoComprobante { get; set; }
        public string? NumeroComprobante { get; set; }

        [Required(ErrorMessage = "El total es obligatorio")]
        public decimal Total { get; set; }

        public string? CAE { get; set; }
        public DateTime? VTO { get; set; }
        public decimal? Bonificacion { get; set; }
        public decimal? PorcentajeBonif { get; set; }
        public decimal? Anticipo { get; set; }
        public decimal? ContraEntrega { get; set; }
        public int? Cuotas { get; set; }
        public decimal? ValorCuota { get; set; }
        public int? Vendedor_Id { get; set; }
        public decimal? GastosEnvio { get; set; }
        public bool EsElectronica { get; set; } = true;
        public bool EsPresupuesto { get; set; } = false;

        [Required(ErrorMessage = "Los detalles son obligatorios")]
        public List<ComprobanteDetalleDto> Detalles { get; set; } = new List<ComprobanteDetalleDto>();
    }

    public class UpdateComprobanteDto
    {
        [Required(ErrorMessage = "El cliente es obligatorio")]
        public int Cliente_Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Fecha { get; set; }

        public string? TipoComprobante { get; set; }
        public string? NumeroComprobante { get; set; }

        [Required(ErrorMessage = "El total es obligatorio")]
        public decimal Total { get; set; }

        public string? CAE { get; set; }
        public DateTime? VTO { get; set; }
        public decimal? Bonificacion { get; set; }
        public decimal? PorcentajeBonif { get; set; }
        public decimal? Anticipo { get; set; }
        public decimal? ContraEntrega { get; set; }
        public int? Cuotas { get; set; }
        public decimal? ValorCuota { get; set; }
        public int? Vendedor_Id { get; set; }
        public decimal? GastosEnvio { get; set; }
        public bool EsElectronica { get; set; } = true;
        public bool EsPresupuesto { get; set; } = false;

        [Required(ErrorMessage = "Los detalles son obligatorios")]
        public List<ComprobanteDetalleDto> Detalles { get; set; } = new List<ComprobanteDetalleDto>();
    }

    public class ComprobanteDetalleDto
    {
        [Required(ErrorMessage = "El artículo es obligatorio")]
        public int Articulo_Id { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio unitario es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0")]
        public decimal Precio_Unitario { get; set; }

        [Required(ErrorMessage = "El subtotal es obligatorio")]
        public decimal Subtotal { get; set; }
    }
}
