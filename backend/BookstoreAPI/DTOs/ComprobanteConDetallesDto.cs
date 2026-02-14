namespace BookstoreAPI.DTOs
{
    public class ComprobanteConDetallesDto
    {
        public int Id { get; set; }
        public int Cliente_Id { get; set; }
        public string? ClienteNombre { get; set; }
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
        public string? VendedorNombre { get; set; }
        public decimal? GastosEnvio { get; set; }
        public bool EsElectronica { get; set; }
        public bool EsPresupuesto { get; set; }

        // Estado del comprobante: PEN (Pendiente), PAG (Pagada), CAN (Cancelado)
        public string? Estado { get; set; }

        // Relación con comprobante asociado
        public int? ComprobanteAsociado_Id { get; set; }
        public string? ComprobanteAsociadoNumero { get; set; }  // Para NC: número de la factura

        // Para Facturas: indica si fue cancelada por una NC
        public bool EstaCancelado { get; set; }
        public string? NotaCreditoNumero { get; set; }  // Número de la NC que canceló esta factura

        public List<ComprobanteDetalleConArticuloDto> Detalles { get; set; } = new List<ComprobanteDetalleConArticuloDto>();
    }

    public class ComprobanteDetalleConArticuloDto
    {
        public int Id { get; set; }
        public int Articulo_Id { get; set; }
        public string? ArticuloCodigo { get; set; }
        public string? ArticuloDescripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio_Unitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
