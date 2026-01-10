using BookstoreAPI.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BookstoreAPI.Services.Pdf
{
    public class ArticulosVendidosZonaPdfService : IArticulosVendidosZonaPdfService
    {
        public byte[] GenerarPdf(ArticulosVendidosZonaReporteDto reporte)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(GenerarHeader(reporte.ZonaNombre));
                    page.Content().Element(GenerarDetalle(reporte.Items));
                });
            });

            return document.GeneratePdf();
        }

        private Action<IContainer> GenerarHeader(string zonaNombre) => container =>
        {
            container.Column(column =>
            {
                column.Item().BorderTop(2).BorderBottom(2).Row(row =>
                {
                    row.RelativeItem(80).Padding(2)
                        .Column(c =>
                        {
                            c.Item().Text("EDICIONES LIBER de Roberto Passarelli y Marcos E. Passarelli S.H").FontSize(12).Bold();
                            c.Item().Text("Av. Asamblea 1442 P 7 Dto 20 - CP: C1406HVR - C.A.B.A.");
                            c.Item().Text($"LISTADO DE ARTICULOS VENDIDOS POR ZONA").Bold();
                            c.Item().Text($"Zona: {zonaNombre}").FontSize(10);
                            c.Item().Text($"Período: Últimos 3 años").FontSize(9);
                        });

                    row.RelativeItem(20).Padding(2)
                        .AlignRight()
                        .Column(c =>
                        {
                            c.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").FontSize(9);
                            c.Item().Text(text =>
                            {
                                text.Span("Página: ");
                                text.CurrentPageNumber();
                            });
                        });
                });
            });
        };

        private Action<IContainer> GenerarDetalle(List<ArticuloVendidoZonaItemDto> items) => container =>
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(0.5f);  // V (vendedor inicial)
                    columns.RelativeColumn(1f);    // Código
                    columns.RelativeColumn(2.5f);  // Razón Social
                    columns.RelativeColumn(2f);    // Dirección
                    columns.RelativeColumn(2f);    // Dir. Comercial
                    columns.RelativeColumn(2.5f);  // Artículo
                    columns.RelativeColumn(1f);    // Fecha
                    columns.RelativeColumn(1.5f);  // Nro Factura
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).Padding(3).Text("V").Bold();
                    header.Cell().BorderBottom(1).Padding(3).Text("Código").Bold();
                    header.Cell().BorderBottom(1).Padding(3).Text("Razón Social").Bold();
                    header.Cell().BorderBottom(1).Padding(3).Text("Dirección").Bold();
                    header.Cell().BorderBottom(1).Padding(3).Text("Dir. Comercial").Bold();
                    header.Cell().BorderBottom(1).Padding(3).Text("Artículo").Bold();
                    header.Cell().BorderBottom(1).Padding(3).Text("Fecha").Bold();
                    header.Cell().BorderBottom(1).Padding(3).Text("Nro. Factura").Bold();
                });

                if (items.Count == 0)
                {
                    table.Cell().ColumnSpan(8).Element(CellStyleBody).PaddingTop(10).Text("No se encontró información!");
                }

                string clienteActual = "";
                bool alternarColor = false;
                bool esPrimerRegistroCliente = false;

                foreach (var item in items)
                {
                    // Cambiar color cuando cambia el cliente
                    if (item.RazonSocial != clienteActual)
                    {
                        clienteActual = item.RazonSocial;
                        alternarColor = !alternarColor;
                        esPrimerRegistroCliente = true;
                    }
                    else
                    {
                        esPrimerRegistroCliente = false;
                    }

                    Func<IContainer, IContainer> cellStyle = alternarColor ? CellStyleBodyAlt : CellStyleBody;

                    // Solo mostrar vendedor, código, razón social y direcciones en el primer registro del cliente
                    table.Cell().Element(cellStyle).Text(esPrimerRegistroCliente ? item.VendedorInicial : "");
                    table.Cell().Element(cellStyle).Text(esPrimerRegistroCliente ? item.CodigoCliente : "");
                    table.Cell().Element(cellStyle).Text(esPrimerRegistroCliente ? TruncateText(item.RazonSocial, 35) : "");
                    table.Cell().Element(cellStyle).Text(esPrimerRegistroCliente ? TruncateText(item.Direccion, 30) : "");
                    table.Cell().Element(cellStyle).Text(esPrimerRegistroCliente ? TruncateText(item.DireccionComercial, 30) : "");
                    table.Cell().Element(cellStyle).Text(TruncateText(item.DescripcionArticulo, 40));
                    table.Cell().Element(cellStyle).Text($"{item.FechaFactura:dd/MM/yyyy}");
                    table.Cell().Element(cellStyle).Text(item.NumeroFactura);
                }

                // Total de registros
                table.Cell().ColumnSpan(8).BorderTop(2).Padding(5).Text($"Total de registros: {items.Count}").Bold();
            });
        };

        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }

        private static IContainer CellStyleBody(IContainer container) =>
            container.Padding(2).DefaultTextStyle(x => x.FontSize(8));

        private static IContainer CellStyleBodyAlt(IContainer container) =>
            container.Padding(2).Background(Colors.Grey.Lighten4).DefaultTextStyle(x => x.FontSize(8));
    }
}
