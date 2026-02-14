namespace BookstoreAPI.Models
{
    public class Comprobante
    {
        public int Id { get; set; }
        public int Cliente_Id { get; set; }
        public DateTime Fecha { get; set; }
        public string? TipoComprobante { get; set; }
        public string? NumeroComprobante { get; set; }
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

        // Relación con comprobante asociado (para NC: ID de la factura que cancela)
        public int? ComprobanteAsociado_Id { get; set; }

        // Estado del comprobante: PEN (Pendiente), PAG (Pagada), CAN (Cancelado)
        public string? Estado { get; set; } = "PEN";
    }
}
