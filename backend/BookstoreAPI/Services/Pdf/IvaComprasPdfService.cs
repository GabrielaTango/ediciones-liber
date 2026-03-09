using BookstoreAPI.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BookstoreAPI.Services.Pdf
{
    public class IvaComprasPdfService : IIvaComprasPdfService
    {
        public byte[] GenerarPdf(List<IvaComprasDto> compras, DateTime fechaDesde, DateTime fechaHasta)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Element(GenerarHeader(fechaDesde, fechaHasta));
                    page.Content().Element(GenerarDetalle(compras));
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
                            c.Item().Text($"C.U.I.T. 30-71417888-8    LIBRO DE I.V.A - COMPRAS").FontSize(10);
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

        private Action<IContainer> GenerarDetalle(List<IvaComprasDto> compras) => container =>
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);   // Fecha
                    columns.RelativeColumn(0.6f); // Tipo
                    columns.RelativeColumn(1);   // Número
                    columns.RelativeColumn(2);   // Proveedor
                    columns.RelativeColumn(1);   // CUIT
                    columns.RelativeColumn(1);   // Débitos
                    columns.RelativeColumn(1);   // Créditos
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).Padding(5).Text("Fecha").Bold();
                    header.Cell().BorderBottom(1).Padding(5).Text("Tipo").Bold();
                    header.Cell().BorderBottom(1).Padding(5).Text("Número").Bold();
                    header.Cell().BorderBottom(1).Padding(5).Text("Proveedor").Bold();
                    header.Cell().BorderBottom(1).Padding(5).Text("CUIT").Bold();
                    header.Cell().BorderBottom(1).Padding(5).Text("Débitos").Bold();
                    header.Cell().BorderBottom(1).Padding(5).Text("Créditos").Bold();
                });

                if (compras.Count == 0)
                {
                    table.Cell().ColumnSpan(7).Element(CellStyleBody).PaddingTop(2).Text("No se encontró información!");
                }

                decimal totalDebitos = 0;
                decimal totalCreditos = 0;
                int cnt = 0;
                int cantidad = 49;

                for (int i = 0; i < compras.Count; i++)
                {
                    var compra = compras[i];
                    var esCredito = compra.TipoComprobante == "NC";
                    var debito = esCredito ? 0M : compra.Total;
                    var credito = esCredito ? compra.Total : 0M;

                    totalDebitos += debito;
                    totalCreditos += credito;

                    if (cnt == cantidad)
                    {
                        table.Cell().Text("");
                        table.Cell().Text("");
                        table.Cell().Text("");
                        table.Cell().Element(CellStyleBody).Text("Transporte").Bold();
                        table.Cell().Text("");
                        table.Cell().Element(CellStyleBody).AlignRight().Text($"${totalDebitos:N2}").Bold();
                        table.Cell().Element(CellStyleBody).AlignRight().Text($"${totalCreditos:N2}").Bold();
                        cnt = 0;
                        cantidad = 48;
                    }
                    else
                    {
                        table.Cell().Element(CellStyleBody).Text($"{compra.Fecha:dd/MM/yyyy}");
                        table.Cell().Element(CellStyleBody).Text($"{compra.TipoComprobante}");
                        table.Cell().Element(CellStyleBody).Text($"{compra.NumeroComprobante ?? "-"}");
                        table.Cell().Element(CellStyleBody).Text($"{compra.Nombre ?? "-"}");
                        table.Cell().Element(CellStyleBody).Text($"{compra.Cuit ?? "-"}");
                        table.Cell().Element(CellStyleBody).AlignRight().Text($"${debito:N2}");
                        table.Cell().Element(CellStyleBody).AlignRight().Text($"${credito:N2}");
                        cnt++;
                    }
                }

                // Totales finales
                table.Cell().ColumnSpan(5).BorderTop(2).Padding(5).Text("TOTALES").Bold().AlignRight();
                table.Cell().BorderTop(2).Element(CellStyleBody).AlignRight().Text($"${totalDebitos:N2}").Bold();
                table.Cell().BorderTop(2).Element(CellStyleBody).AlignRight().Text($"${totalCreditos:N2}").Bold();
            });
        };

        private static IContainer CellStyleBody(IContainer container) =>
            container.DefaultTextStyle(x => x.FontSize(9));
    }
}
