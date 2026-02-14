using BookstoreAPI.Models;
using BookstoreAPI.Models.Afip;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BookstoreAPI.Services.Pdf
{
    public class CuotaPdfService : ICuotaPdfService
    {
        private readonly AfipConfig _config;
        private readonly ILogger<CuotaPdfService> _logger;

        public CuotaPdfService(
            IOptions<AfipConfig> config,
            ILogger<CuotaPdfService> logger)
        {
            _config = config.Value;
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
            container.Column(column =>
            {
                column.Item().Border(1).Padding(10).Row(row =>
                {
                    // Columna izquierda - Datos del cliente
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(text =>
                        {
                            text.Span($"Cliente: {cliente.Nombre} - {cliente.Id}").Bold();
                        });
                        c.Item().Text($"Domicilio Com: {cliente.DomicilioComercial ?? "-"}").FontSize(10);
                        c.Item().Text($"Dirección Part: {cliente.DomicilioParticular ?? "-"}").FontSize(10);
                        c.Item().Text($"Teléfono: {cliente.Telefono ?? cliente.TelefonoMovil ?? "-"}").FontSize(10);
                    });

                    // Columna derecha - Datos del comprobante
                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().Text($"Email: {cliente.EMail ?? "-"}").FontSize(10);
                        c.Item().Text($"Documento: {cliente.NroDocumento ?? "-"}").FontSize(10);
                        c.Item().Text($"Factura: {comprobante.NumeroComprobante}").FontSize(10);
                        c.Item().Text($"Fecha: {comprobante.Fecha:dd/MM/yyyy}").FontSize(10);
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

                foreach (var cuota in cuotas.OrderBy(c => c.NumeroCuota))
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

                    // Encabezado del cupón
                    c.Item().AlignCenter().Width(60).Image("./Images/LiberLogo.png");
                    //c.Item().Border(1).Background(Colors.Grey.Lighten3)
                        //.Padding(5).Text("BOOKSTORE APP").FontSize(11).Bold().AlignCenter();

                    // Datos del cliente
                    c.Item().Text($"Sr/a: {cliente.Nombre}").FontSize(9);

                    // Mes y año de vencimiento
                    c.Item().Text($"Mes: {cuota.Fecha?.ToString("MM/yyyy") ?? "-"}").FontSize(9);

                    // Monto y número de cuota
                    c.Item().Row(row =>
                    {
                        row.RelativeItem(70).Column(col =>
                        {
                            col.Item().Text($"Cuota: ${cuota.Importe:N2}").FontSize(10).Bold();
                        });

                        row.RelativeItem(30).Column(col =>
                        {
                            col.Item().AlignRight().Padding(4)
                                .Text(etiquetaCuota).FontSize(16).Bold();
                        });
                    });

                    // Información adicional
                    c.Item().PaddingTop(3).Text($"Factura: {comprobante.NumeroComprobante}").FontSize(7);
                });
            });
        }
    }
}
