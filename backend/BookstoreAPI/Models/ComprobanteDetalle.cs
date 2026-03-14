namespace BookstoreAPI.Models
{
    public class ComprobanteDetalle
    {
        public int Id { get; set; }
        public int Factura_Id { get; set; }
        public int Articulo_Id { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio_Unitario { get; set; }
        public decimal Subtotal { get; set; }
        public string? ArticuloDescripcion { get; set; }
    }
}
