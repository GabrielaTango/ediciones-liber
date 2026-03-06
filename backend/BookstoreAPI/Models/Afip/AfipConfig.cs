namespace BookstoreAPI.Models.Afip
{
    public class AfipConfig
    {
        public string CUIT { get; set; } = string.Empty;
        public string PfxPath { get; set; } = string.Empty;
        public string PfxPassword { get; set; } = string.Empty;
        public string CrtPath { get; set; } = string.Empty;
        public string KeyPath { get; set; } = string.Empty;
        public string WsaaUrl { get; set; } = string.Empty;
        public string WsfevUrl { get; set; } = string.Empty;
        public int PuntoVenta { get; set; }
        public bool IsProduction { get; set; }

        // Certificados cargados desde la DB (base64 -> bytes)
        public byte[]? CrtBytes { get; set; }
        public byte[]? KeyBytes { get; set; }
    }
}
