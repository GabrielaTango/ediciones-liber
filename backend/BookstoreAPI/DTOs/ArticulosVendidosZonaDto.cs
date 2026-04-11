namespace BookstoreAPI.DTOs
{
    public class ArticuloVendidoZonaItemDto
    {
        public string VendedorInicial { get; set; } = string.Empty;
        public string CodigoCliente { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string DireccionComercial { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string DescripcionArticulo { get; set; } = string.Empty;
        public DateTime FechaFactura { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
    }

    public class ArticulosVendidosZonaReporteDto
    {
        public int? ZonaId { get; set; }
        public string ZonaNombre { get; set; } = string.Empty;
        public List<ArticuloVendidoZonaItemDto> Items { get; set; } = new();
    }
}
