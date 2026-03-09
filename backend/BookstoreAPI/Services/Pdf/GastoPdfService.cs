using BookstoreAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BookstoreAPI.Services.Pdf
{
    public class GastoPdfService : IGastoPdfService
    {
        public byte[] GenerarPdf(List<Gasto> gastos, DateTime fechaDesde, DateTime fechaHasta)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(GenerarHeader(fechaDesde, fechaHasta));
                    page.Content().Element(GenerarDetalle(gastos));
                });
            });

            return document.GeneratePdf();
        }

        private Action<IContainer> GenerarHeader(DateTime fechaDesde, DateTime fechaHasta) => container =>
        {
            container.Column(column =>
            {
                column.Item().BorderTop(2).BorderBottom(2).Row(row =>
                {
                    row.ConstantItem(60).Padding(2)
                        .AlignCenter()
                        .AlignMiddle()
                        .Image("./Images/LiberLogo.png");

                    row.RelativeItem().Padding(2)
                        .Column(c =>
                        {
                            c.Item().Text("de Roberto Passarelli y Marcos E. Passarelli S.H").FontSize(10).Bold();
                            c.Item().Text("Av. Asamblea 1442 P 7 Dto 20 - CP: C1406HVR - C.A.B.A.").FontSize(8);
                            c.Item().Text($"C.U.I.T. 30-71417888-8    LISTADO DE GASTOS").FontSize(10);
                            c.Item().Text($"Período: {fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}").FontSize(10);
                        });

                    row.ConstantItem(100).Padding(2)
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

        private Action<IContainer> GenerarDetalle(List<Gasto> gastos) => container =>
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.2f);  // Fecha
                    columns.RelativeColumn(1.5f);  // Nro Comprobante
                    columns.RelativeColumn(1.5f);  // Categoría
                    columns.RelativeColumn(3f);    // Descripción
                    columns.RelativeColumn(1.2f);  // Importe
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).Padding(3).Text("Fecha").Bold();
                    header.Cell().BorderBottom(1).Padding(3).Text("Nro. Comprobante").Bold();
                    header.Cell().BorderBottom(1).Padding(3).Text("Categoría").Bold();
                    header.Cell().BorderBottom(1).Padding(3).Text("Descripción").Bold();
                    header.Cell().BorderBottom(1).Padding(3).AlignRight().Text("Importe").Bold();
                });

                if (gastos.Count == 0)
                {
                    table.Cell().ColumnSpan(5).Element(CellStyleBody).PaddingTop(10).Text("No se encontró información!");
                }

                bool alternar = false;
                foreach (var gasto in gastos)
                {
                    Func<IContainer, IContainer> cellStyle = alternar ? CellStyleBodyAlt : CellStyleBody;

                    table.Cell().Element(cellStyle).Text($"{gasto.Fecha:dd/MM/yyyy}");
                    table.Cell().Element(cellStyle).Text(gasto.NroComprobante);
                    table.Cell().Element(cellStyle).Text(TruncateText(gasto.Categoria, 30));
                    table.Cell().Element(cellStyle).Text(TruncateText(gasto.Descripcion, 60));
                    table.Cell().Element(cellStyle).AlignRight().Text($"${gasto.Importe:N2}");

                    alternar = !alternar;
                }

                // Total
                var total = gastos.Sum(g => g.Importe);
                table.Cell().ColumnSpan(4).BorderTop(2).Padding(5).Text($"Total de registros: {gastos.Count}").Bold();
                table.Cell().BorderTop(2).Padding(5).AlignRight().Text($"${total:N2}").Bold();
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
