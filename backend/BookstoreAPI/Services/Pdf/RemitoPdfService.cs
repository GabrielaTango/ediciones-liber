using BookstoreAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BookstoreAPI.Services.Pdf
{
    public class RemitoPdfService : IRemitoPdfService
    {
        private readonly ILogger<RemitoPdfService> _logger;

        public RemitoPdfService(ILogger<RemitoPdfService> logger)
        {
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerarRemitoPdf(Remito remito)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(25);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Content().Element(c => ComposeRemito(c, remito));
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de remito");
                throw;
            }
        }

        private void ComposeRemito(IContainer container, Remito remito)
        {
            container.Column(column =>
            {
                // ===== ENCABEZADO =====
                column.Item().Row(row =>
                {
                    // Columna izquierda - Datos de la empresa
                    row.RelativeItem(6).Column(leftCol =>
                    {
                        leftCol.Item().Text("E D I C I O N E S").FontSize(10).LetterSpacing(0.1f);
                        leftCol.Item().Text("LIBER").Bold().FontSize(32);
                        leftCol.Item().PaddingTop(3).Text("de Roberto José Passarelli y Marcos E. Passarelli S.H.").FontSize(8);
                        leftCol.Item().PaddingTop(5).Text("Av. Asamblea 1442 P. 7 Dto. 20 - C.P.: C1406HVR - CABA").FontSize(8);
                        leftCol.Item().Text("Cel: 011 55012902 Marcos").FontSize(8);
                        leftCol.Item().Text("Cel: 01135772183 Roberto").FontSize(8);
                        leftCol.Item().PaddingTop(3).Text("IVA EXENTO").FontSize(8);
                    });

                    // Columna derecha - Datos del remito (con borde)
                    row.RelativeItem(4).Border(1).Padding(10).Column(rightCol =>
                    {
                        // Encabezado REMITO
                        rightCol.Item().Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("REMITO Nº").Bold().FontSize(11);
                                c.Item().Text(remito.Numero).Bold().FontSize(14);
                            });
                            // Cuadro X
                            r.ConstantItem(30).Height(30).Border(1).AlignCenter().AlignMiddle()
                                .Text("X").Bold().FontSize(16);
                        });

                        rightCol.Item().PaddingTop(8).Text($"Fecha: {remito.Fecha:dd/MM/yyyy}").Bold().FontSize(10);
                        rightCol.Item().PaddingTop(5).Text("C.U.I.T.: 30-71417888-8").FontSize(9);
                        rightCol.Item().Text("Ingr. Brtos.: EXENTO").FontSize(9);
                        rightCol.Item().Text("Inicio Activ.: 01/09/2013").FontSize(9);
                    });
                });

                column.Item().PaddingVertical(10).LineHorizontal(1);

                // ===== DATOS DEL TRANSPORTE =====
                column.Item().PaddingBottom(5).Column(transporteCol =>
                {
                    transporteCol.Item().Row(r =>
                    {
                        r.ConstantItem(70).Text("Transporte:").Bold();
                        r.RelativeItem().Text(remito.TransporteNombre?.ToUpper() ?? "-").Bold();
                    });
                    transporteCol.Item().Row(r =>
                    {
                        r.ConstantItem(70).Text("Dirección:");
                        r.RelativeItem().Text($"{remito.TransporteDireccion ?? "-"} ({remito.TransporteLocalidad ?? ""})");
                    });
                });

                column.Item().LineHorizontal(1);

                // ===== DATOS DEL DESTINATARIO =====
                column.Item().PaddingVertical(10).Column(clienteCol =>
                {
                    clienteCol.Item().Text("CAJAS CON LIBROS PARA ENTREGAR A:").Bold().FontSize(11);
                    clienteCol.Item().PaddingTop(8).Text($"{remito.ClienteNombre?.ToUpper() ?? "-"}, C.U.I.T.: {remito.ClienteCuit ?? "-"}").Bold().FontSize(11);
                    clienteCol.Item().PaddingTop(3).Text(remito.ClienteDomicilio?.ToUpper() ?? "-").FontSize(10);
                    clienteCol.Item().Text($"{remito.ClienteLocalidad?.ToUpper() ?? "-"} C.P.: {remito.ClienteCodigoPostal ?? "-"}").FontSize(10);
                    clienteCol.Item().Text(remito.ClienteProvincia?.ToUpper() ?? "-").FontSize(10);
                });

                column.Item().LineHorizontal(1);

                // ===== CANTIDAD DE BULTOS Y VALOR =====
                column.Item().PaddingVertical(15).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Cantidad de Bultos: {remito.CantidadBultos}").Bold().FontSize(12);
                    });
                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().Text($"Valor Declarado $: {remito.ValorDeclarado:N2}").Bold().FontSize(12);
                    });
                });

                column.Item().LineHorizontal(1);

                // ===== OBSERVACIONES =====
                if (!string.IsNullOrWhiteSpace(remito.Observaciones))
                {
                    column.Item().PaddingVertical(10).Border(1).Background(Colors.Grey.Lighten4).Padding(10).Column(obsCol =>
                    {
                        obsCol.Item().Text("OBSERVACIONES:").Bold().FontSize(10);
                        obsCol.Item().PaddingTop(5).Text(remito.Observaciones).FontSize(10);
                    });

                    column.Item().LineHorizontal(1);
                }

                // ===== SECCIÓN DE FIRMA =====
                column.Item().PaddingTop(20).Column(firmaCol =>
                {
                    firmaCol.Item().Text("Recibí Conforme:").FontSize(10);

                    // Espacio para firma
                    firmaCol.Item().PaddingTop(40).PaddingBottom(10).LineHorizontal(0.5f);

                    // Tabla de firma
                    firmaCol.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Encabezados
                        table.Cell().Border(1).Padding(8).AlignCenter().Text("Firma").Bold();
                        table.Cell().Border(1).Padding(8).AlignCenter().Text("Aclaración").Bold();
                        table.Cell().Border(1).Padding(8).AlignCenter().Text("Documento").Bold();

                        // Celdas vacías para completar
                        table.Cell().Border(1).Height(40);
                        table.Cell().Border(1).Height(40);
                        table.Cell().Border(1).Height(40);
                    });
                });
            });
        }

        public byte[] GenerarEtiquetasPdf(Remito remito)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    // Calcular páginas necesarias (3 etiquetas por página)
                    var etiquetasPorPagina = 3;
                    var totalPaginas = (int)Math.Ceiling((double)remito.CantidadBultos / etiquetasPorPagina);

                    for (int pagina = 0; pagina < totalPaginas; pagina++)
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(15);
                            page.DefaultTextStyle(x => x.FontSize(10));

                            page.Content().Column(column =>
                            {
                                column.Spacing(10);

                                var inicio = pagina * etiquetasPorPagina;
                                var fin = Math.Min(inicio + etiquetasPorPagina, remito.CantidadBultos);

                                for (int i = inicio; i < fin; i++)
                                {
                                    var numeroBulto = i + 1;
                                    column.Item().Element(c => RenderEtiqueta(c, remito, numeroBulto));
                                }
                            });
                        });
                    }
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de etiquetas");
                throw;
            }
        }

        public byte[] GenerarRemitoCompletoConEtiquetas(Remito remito)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    // ===== REMITO POR TRIPLICADO (3 páginas) =====
                    for (int copia = 1; copia <= 3; copia++)
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(25);
                            page.DefaultTextStyle(x => x.FontSize(10));

                            page.Content().Element(c => ComposeRemito(c, remito));
                        });
                    }

                    // ===== ETIQUETAS =====
                    var etiquetasPorPagina = 3;
                    var totalPaginasEtiquetas = (int)Math.Ceiling((double)remito.CantidadBultos / etiquetasPorPagina);

                    for (int pagina = 0; pagina < totalPaginasEtiquetas; pagina++)
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(15);
                            page.DefaultTextStyle(x => x.FontSize(10));

                            page.Content().Column(column =>
                            {
                                column.Spacing(10);

                                var inicio = pagina * etiquetasPorPagina;
                                var fin = Math.Min(inicio + etiquetasPorPagina, remito.CantidadBultos);

                                for (int i = inicio; i < fin; i++)
                                {
                                    var numeroBulto = i + 1;
                                    column.Item().Element(c => RenderEtiqueta(c, remito, numeroBulto));
                                }
                            });
                        });
                    }
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF completo de remito con etiquetas");
                throw;
            }
        }

        private void RenderEtiqueta(IContainer container, Remito remito, int numeroBulto)
        {
            container.Border(1).Column(column =>
            {
                // ===== SECCIÓN SUPERIOR: ENVÍO DE + BULTOS =====
                column.Item().Padding(10).Row(headerRow =>
                {
                    // Columna izquierda - Envío de + datos empresa
                    headerRow.RelativeItem(6).Column(envioCol =>
                    {
                        envioCol.Item().Text("Envío de:").Bold().FontSize(10);
                        envioCol.Item().PaddingTop(3).Text("E D I C I O N E S").FontSize(8);
                        envioCol.Item().Text("LIBER").Bold().FontSize(20);
                        envioCol.Item().Text("de Roberto José Passarelli y Marcos E. Passarelli S.H.").FontSize(7);
                        envioCol.Item().Text("Av. Asamblea 1442 P. 7 Dto. 20 - C.P.: C1406HVR - CABA").FontSize(7);
                        envioCol.Item().Text("I.V.A. EXENTO").FontSize(7);
                        envioCol.Item().Text("Cel: 011 55012902 Marcos").FontSize(7);
                        envioCol.Item().Text("Cel: 01135772183 Roberto").FontSize(7);
                    });

                    // Columna derecha - Número de bultos
                    headerRow.RelativeItem(4).AlignRight().AlignMiddle().Column(bultoCol =>
                    {
                        bultoCol.Item().AlignRight().Text($"Bultos: {numeroBulto}/{remito.CantidadBultos}")
                            .Bold().FontSize(28);
                    });
                });

                // ===== LÍNEA SEPARADORA =====
                column.Item().LineHorizontal(1);

                // ===== DATOS DEL DESTINATARIO =====
                column.Item().Padding(12).Column(content =>
                {
                    content.Spacing(6);

                    content.Item().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(11));
                        text.Span("Sres: ").Bold();
                        text.Span(remito.ClienteNombre?.ToUpper() ?? "-").Bold();
                    });

                    content.Item().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(11));
                        text.Span("Domicilio: ").Bold();
                        text.Span(remito.ClienteDomicilio?.ToUpper() ?? "-");
                    });

                    content.Item().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(11));
                        text.Span("Localidad: ").Bold();
                        text.Span($"{remito.ClienteLocalidad?.ToUpper() ?? "-"} ({remito.ClienteCodigoPostal ?? "-"})");
                    });

                    content.Item().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(11));
                        text.Span("Provincia: ").Bold();
                        text.Span(remito.ClienteProvincia?.ToUpper() ?? "-");
                    });
                });
            });
        }
    }
}
