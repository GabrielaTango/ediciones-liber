namespace BookstoreAPI.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int? Zona_Id { get; set; }
        public int? SubZona_Id { get; set; }
        public int? Vendedor_Id { get; set; }
        public string? DomicilioComercial { get; set; }
        public string? DomicilioParticular { get; set; }
        public int? Provincia_Id { get; set; }
        public string? CodigoPostal { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaInha { get; set; }
        public bool SoloContado { get; set; }
        public string? Telefono { get; set; }
        public string? TelefonoMovil { get; set; }
        public string? EMail { get; set; }
        public string? Contacto { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NroDocumento { get; set; }
        public string? NroIIBB { get; set; }
        public string? CategoriaIva { get; set; }
        public string? CondicionPago { get; set; }
        public decimal Descuento { get; set; }
        public string? Observaciones { get; set; }
        public string? TipoDocArca { get; set; }

        // Campos de navegación (JOIN)
        public string? ProvinciaDescripcion { get; set; }
        public string? Localidad { get; set; }
        public string? ZonaDescripcion { get; set; }
        public string? SubZonaDescripcion { get; set; }
    }
}
