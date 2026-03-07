namespace BookstoreAPI.DTOs
{
    public class AfipConfigDto
    {
        public string CUIT { get; set; } = string.Empty;
        public string WsaaUrl { get; set; } = string.Empty;
        public string WsfevUrl { get; set; } = string.Empty;
        public int PuntoVenta { get; set; }
        public bool IsProduction { get; set; }
        public bool TieneCrt { get; set; }
        public bool TieneKey { get; set; }
    }

    public class AfipConfigUpdateDto
    {
        public string CUIT { get; set; } = string.Empty;
        public string WsaaUrl { get; set; } = string.Empty;
        public string WsfevUrl { get; set; } = string.Empty;
        public int PuntoVenta { get; set; }
        public bool IsProduction { get; set; }
        public string? CrtBase64 { get; set; }
        public string? KeyBase64 { get; set; }
    }

    public class UltimoComprobanteDto
    {
        public int PuntoVenta { get; set; }
        public int TipoComprobante { get; set; }
        public string TipoComprobanteDescripcion { get; set; } = string.Empty;
        public int UltimoNumero { get; set; }
    }

    public class BackupPathDto
    {
        public string Ruta { get; set; } = string.Empty;
    }

    public class RestoreDto
    {
        public string Archivo { get; set; } = string.Empty;
    }
}
