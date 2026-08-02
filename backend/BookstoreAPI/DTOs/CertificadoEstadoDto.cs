namespace BookstoreAPI.DTOs
{
    public class CertificadoEstadoDto
    {
        /// <summary>Indica si se pudo leer un certificado (DB, archivos CRT/KEY o PFX).</summary>
        public bool TieneCertificado { get; set; }

        /// <summary>"vigente" | "por_vencer" | "vencido" | "sin_certificado" | "error"</summary>
        public string Estado { get; set; } = "sin_certificado";

        public DateTime? FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }

        /// <summary>Días que faltan para el vencimiento. Negativo si ya venció.</summary>
        public int? DiasRestantes { get; set; }

        public string? Subject { get; set; }
        public string? Mensaje { get; set; }
    }
}
