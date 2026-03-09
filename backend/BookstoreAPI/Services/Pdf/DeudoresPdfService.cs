using BookstoreAPI.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BookstoreAPI.Services.Pdf
{
    public class DeudoresPdfService : IDeudoresPdfService
    {
        public byte[] GenerarPdf(DeudoresReporteDto reporte)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var meses = new[] { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
            var periodoTexto = $"{meses[reporte.Mes]} {reporte.Anio}";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(GenerarHeader(periodoTexto));
                    page.Content().Element(GenerarDetalle(reporte));
                });
            });

            return document.GeneratePdf();
        }

        private Action<IContainer> GenerarHeader(string periodoTexto) => container =>
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
                            c.Item().Text($"C.U.I.T. 30-71417888-8    LISTADO DE DEUDORES").FontSize(10);
                            c.Item().Text($"Período: {periodoTexto}").FontSize(10);
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

        private Action<IContainer> GenerarDetalle(DeudoresReporteDto reporte) => container =>
        {
            var periodos = reporte.PeriodosCuotas;
            // Total columns: Nro Comprobante, Razón Social, Vendedor, Cuotas, Total, Saldo, Anticipo + periodos
            var totalCols = (uint)(7 + periodos.Count);

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.5f);  // Nro Comprobante
                    columns.RelativeColumn(2.5f);  // Razón Social
                    columns.RelativeColumn(1f);    // Vendedor
                    columns.RelativeColumn(0.8f);  // Cuotas
                    columns.RelativeColumn(1.2f);  // Total
                    columns.RelativeColumn(1.2f);  // Saldo
                    columns.RelativeColumn(1.2f);  // Anticipo
                    foreach (var _ in periodos)
                    {
                        columns.RelativeColumn(1.2f); // Periodo
                    }
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).Padding(3).Text("Nº Comp.").Bold();
                    header.Cell().BorderBottom(1).Padding(3).Text("Razón Social").Bold();
                    header.Cell().BorderBottom(1).Padding(3).Text("Vend.").Bold();
                    header.Cell().BorderBottom(1).Padding(3).AlignCenter().Text("Ctas").Bold();
                    header.Cell().BorderBottom(1).Padding(3).AlignRight().Text("Total").Bold();
                    header.Cell().BorderBottom(1).Padding(3).AlignRight().Text("Saldo").Bold();
                    header.Cell().BorderBottom(1).Padding(3).AlignRight().Text("Antic.").Bold();
                    foreach (var periodo in periodos)
                    {
                        header.Cell().BorderBottom(1).Padding(3).AlignRight().Text(ShortenPeriodo(periodo)).Bold();
                    }
                });

                if (reporte.Deudores.Count == 0)
                {
                    table.Cell().ColumnSpan(totalCols).Element(CellStyleBody).PaddingTop(10).Text("No se encontró información!");
                    return;
                }

                bool alternar = false;
                foreach (var deudor in reporte.Deudores)
                {
                    Func<IContainer, IContainer> cellStyle = alternar ? CellStyleBodyAlt : CellStyleBody;

                    table.Cell().Element(cellStyle).Text(ShortenComprobante(deudor.NumeroComprobante));
                    table.Cell().Element(cellStyle).Text(TruncateText(deudor.RazonSocial, 40));
                    table.Cell().Element(cellStyle).Text(deudor.CodigoVendedor ?? "-");
                    table.Cell().Element(cellStyle).AlignCenter().Text(deudor.CantidadCuotas.ToString());
                    table.Cell().Element(cellStyle).AlignRight().Text($"${deudor.TotalComprobante:N0}");
                    table.Cell().Element(cellStyle).AlignRight().Text($"${deudor.Saldo:N0}").FontColor(deudor.Saldo > 0 ? Colors.Red.Medium : Colors.Green.Medium);
                    table.Cell().Element(cellStyle).AlignRight().Text($"${deudor.Anticipo:N0}");

                    foreach (var periodo in periodos)
                    {
                        var cuota = deudor.Cuotas.FirstOrDefault(c => c.Periodo == periodo);
                        var importePagado = cuota?.ImportePagado ?? 0;
                        table.Cell().Element(cellStyle).AlignRight().Text($"${importePagado:N0}");
                    }

                    alternar = !alternar;
                }

                // Totals row
                table.Cell().ColumnSpan(4).Element(CellStyleBody).BorderTop(1).Text("TOTALES").Bold();
                table.Cell().Element(CellStyleBody).BorderTop(1).AlignRight().Text($"${reporte.Deudores.Sum(d => d.TotalComprobante):N0}").Bold();
                table.Cell().Element(CellStyleBody).BorderTop(1).AlignRight().Text($"${reporte.Deudores.Sum(d => d.Saldo):N0}").Bold().FontColor(Colors.Red.Medium);
                table.Cell().Element(CellStyleBody).BorderTop(1).AlignRight().Text($"${reporte.Deudores.Sum(d => d.Anticipo):N0}").Bold();

                foreach (var periodo in periodos)
                {
                    var total = reporte.Deudores.Sum(d =>
                    {
                        var cuota = d.Cuotas.FirstOrDefault(c => c.Periodo == periodo);
                        return cuota?.ImportePagado ?? 0;
                    });
                    table.Cell().Element(CellStyleBody).BorderTop(1).AlignRight().Text($"${total:N0}").Bold();
                }
            });
        };

        private static string ShortenComprobante(string? nroComprobante)
        {
            if (string.IsNullOrEmpty(nroComprobante)) return "-";
            return nroComprobante.Length > 8 ? nroComprobante.Substring(nroComprobante.Length - 8) : nroComprobante;
        }

        private static string ShortenPeriodo(string periodo)
        {
            if (string.Equals(periodo, "C.Entrega", StringComparison.OrdinalIgnoreCase))
                return "CE";
            // Convierte "MM/YYYY" a "MM/YY"
            if (periodo.Length == 7 && periodo[2] == '/')
                return periodo.Substring(0, 3) + periodo.Substring(5, 2);
            return periodo;
        }

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
