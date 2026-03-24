using BookstoreAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BookstoreAPI.Services.Pdf
{
    public class CuotaPdfService : ICuotaPdfService
    {
        private readonly ILogger<CuotaPdfService> _logger;

        public CuotaPdfService(
            ILogger<CuotaPdfService> logger)
        {
            _logger = logger;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerarCuponesPdf(Comprobante comprobante, Cliente cliente, List<Cuota> cuotas)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Element(c => ComposeHeader(c, cliente, comprobante, cuotas));
                        page.Content().Element(c => ComposeContent(c, cliente, comprobante, cuotas));
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de cupones");
                throw;
            }
        }

        public byte[] GenerarLoteCuponesPdf(List<(Comprobante comprobante, Cliente cliente, List<Cuota> cuotas)> lote)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    foreach (var item in lote)
                    {
                        if (!item.cuotas.Any()) continue;

                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(30);
                            page.DefaultTextStyle(x => x.FontSize(10));

                            page.Header().Element(c => ComposeHeader(c, item.cliente, item.comprobante, item.cuotas));
                            page.Content().Element(c => ComposeContent(c, item.cliente, item.comprobante, item.cuotas));
                        });
                    }
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de lote de cupones");
                throw;
            }
        }

        private void ComposeHeader(IContainer container, Cliente cliente, Comprobante comprobante, List<Cuota> cuotas)
        {

            var fontSize = 11;

            container.Column(column =>
            {
                column.Item().Border(1).Padding(10).Row(row =>
                {
                    // Columna izquierda - Datos del cliente
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(text =>
                        {
                            text.Span($"Cliente: {cliente.Nombre} - {cliente.Id}").FontSize(fontSize).Bold();
                        });
                        c.Item().Text($"Domicilio Com: {cliente.DomicilioComercial ?? "-"}").FontSize(fontSize);
                        c.Item().Text($"Dirección Part: {cliente.DomicilioParticular ?? "-"}").FontSize(fontSize);
                        c.Item().Text($"Teléfono: {cliente.Telefono ?? cliente.TelefonoMovil ?? "-"}").FontSize(fontSize);
                    });

                    // Columna derecha - Datos del comprobante
                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().Text($"Email: {cliente.EMail ?? "-"}").FontSize(fontSize);
                        c.Item().Text($"Documento: {cliente.NroDocumento ?? "-"}").FontSize(fontSize);
                        c.Item().Text($"Factura: {comprobante.NumeroComprobante}").FontSize(fontSize);
                        c.Item().Text($"Fecha: {comprobante.Fecha:dd/MM/yyyy}").FontSize(fontSize);
                    });
                });

                // Resumen de cuotas
                var anticipo = comprobante.Anticipo ?? 0;
                var cuotaCero = cuotas.FirstOrDefault(c => c.NumeroCuota == 0);
                var cuotasRegulares = cuotas.Where(c => c.NumeroCuota > 0).ToList();
                var montoCuotaRegular = cuotasRegulares.FirstOrDefault()?.Importe ?? 0;
                var montoContraEntrega = cuotaCero?.Importe ?? 0;

                var resumenTexto = $"Anticipo: ${anticipo:N2}";
                if (montoContraEntrega > 0)
                    resumenTexto += $" - C.Entrega: ${montoContraEntrega:N2}";
                if (cuotasRegulares.Count > 0)
                    resumenTexto += $" - {cuotasRegulares.Count} Cuotas de ${montoCuotaRegular:N2}";

                column.Item().Border(1).Padding(5)
                    .Text(resumenTexto)
                    .FontSize(12).Bold().AlignCenter();
            });
        }

        private void ComposeContent(IContainer container, Cliente cliente, Comprobante comprobante, List<Cuota> cuotas)
        {
            container.Table(table =>
            {
                // Definir 4 columnas
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                foreach (var cuota in cuotas.Where(c => c.NumeroCuota > 0).OrderByDescending(c => c.NumeroCuota))
                {
                    RenderCuponCell(table, cliente, comprobante, cuota);
                }
            });
        }

        private void RenderCuponCell(TableDescriptor table, Cliente cliente, Comprobante comprobante, Cuota cuota)
        {
            // Determinar etiqueta: "CE" para cuota 0 (contraentrega), número para el resto
            var etiquetaCuota = cuota.NumeroCuota == 0 ? "CE" : cuota.NumeroCuota.ToString();

            table.Cell().Padding(5).Element(container =>
            {
                container.Border(1).Padding(5).Column(c =>
                {
                    c.Spacing(3);

                    // Encabezado: logo a la izquierda, nro cuota a la derecha
                    c.Item().Row(row =>
                    {
                        row.ConstantItem(60).AlignCenter().Image("./Images/LiberLogo.png");
                        row.RelativeItem().AlignRight().AlignMiddle()
                            .Text(etiquetaCuota).FontSize(16).Bold();
                    });

                    // Datos del cliente
                    c.Item().Text(TruncateText($"Sr/a: {cliente.Nombre}", 25)).FontSize(9);

                    // Mes y año de vencimiento
                    c.Item().Text($"Mes: {cuota.Fecha?.ToString("MM/yyyy") ?? "-"}").FontSize(9);

                    // Monto
                    c.Item().AlignRight().Text($"${cuota.Importe:N2}").FontSize(10).Bold();

                    // Información adicional
                    c.Item().AlignRight().PaddingTop(3).Text($"Factura: {comprobante.NumeroComprobante}").FontSize(7);
                });
            });
        }

        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }
    }
}
