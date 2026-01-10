using BookstoreAPI.Models;
using BookstoreAPI.Models.Afip;
using BookstoreAPI.Services.Afip;
using Microsoft.Extensions.Options;
using MySqlX.XDevAPI;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BookstoreAPI.Services.Pdf
{
    public class ComprobantePdfService : IComprobantePdfService
    {
        private readonly AfipConfig _config;
        private readonly IAfipQrService _qrService;
        private readonly ILogger<ComprobantePdfService> _logger;
        private int sizeImporte = 8;
        private int sizeCuotas = 12;

        public ComprobantePdfService(
            IOptions<AfipConfig> config,
            IAfipQrService qrService,
            ILogger<ComprobantePdfService> logger)
        {
            _config = config.Value;
            _qrService = qrService;
            _logger = logger;

            // Configurar licencia de QuestPDF (Community para uso no comercial)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerarPdf(Comprobante comprobante, Cliente cliente, List<ComprobanteDetalle> detalles)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    // Comprobante por triplicado (3 páginas)
                    for (int copia = 1; copia <= 3; copia++)
                    {
                        var esPrimeraHoja = copia == 1;
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(2, Unit.Centimetre);
                            page.DefaultTextStyle(x => x.FontSize(10));

                            page.Header().Element(h => ComposeHeader(h, comprobante));
                            page.Content().Element(c => ComposeContent(c, comprobante, cliente, detalles));
                            page.Footer().Element(f => ComposeFooter(f, comprobante, cliente, detalles, esPrimeraHoja));
                        });
                    }
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF del comprobante");
                throw;
            }
        }

        private void ComposeHeader(IContainer container, Comprobante comprobante)
        {
            var tipoTexto = ObtenerTipoComprobanteCompleto(comprobante.TipoComprobante);
            var letraComprobante = ObtenerLetraComprobante(comprobante.TipoComprobante);
            var esPresupuesto = comprobante.TipoComprobante == "PRE" || comprobante.EsPresupuesto;

            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        // Tipo de comprobante con letra
                        column.Item().Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().AlignCenter().PaddingVertical(10).Border(1).Background(Colors.Grey.Lighten3)
                                    .Text(letraComprobante).FontSize(16).Bold();
                            });
                        });
                        column.Item().Width(100).Image("./Images/LiberLogo.png");
                        column.Item().Text($"CUIT: {_config.CUIT}").FontSize(10);
                        column.Item().Text("Dirección: Calle Falsa 123").FontSize(9);
                        column.Item().Text("Tel: (011) 1234-5678").FontSize(9);
                    });
                });

                // Tipo de comprobante (FACTURA / NOTA DE CRÉDITO / PRESUPUESTO)
                col.Item().PaddingTop(10).AlignCenter().Text(tipoTexto).FontSize(14).Bold();

                // Leyenda para presupuestos
                if (esPresupuesto)
                {
                    col.Item().PaddingTop(5).AlignCenter()
                        .Text("COMPROBANTE NO VÁLIDO COMO FACTURA")
                        .FontSize(12).Bold().FontColor(Colors.Red.Medium);
                }
            });
        }

        private void ComposeContent(IContainer container, Comprobante comprobante, Cliente cliente, List<ComprobanteDetalle> detalles)
        {
            container.Column(column =>
            {
                column.Item().PaddingVertical(10);

                // Información del comprobante y cliente
                column.Item().Row(row =>
                {
                    // Datos del comprobante
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("DATOS DEL COMPROBANTE").FontSize(12).Bold();
                        col.Item().PaddingTop(5).Text($"Número: {comprobante.NumeroComprobante}");
                        col.Item().Text($"Fecha: {comprobante.Fecha:dd/MM/yyyy}");

                        if (!string.IsNullOrEmpty(comprobante.CAE))
                        {
                            col.Item().Text($"CAE: {comprobante.CAE}");
                            col.Item().Text($"Vto. CAE: {comprobante.VTO:dd/MM/yyyy}");
                        }
                    });

                    // Datos del cliente
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("DATOS DEL CLIENTE").FontSize(12).Bold();
                        col.Item().PaddingTop(5).Text($"Nombre: {cliente.Nombre}");
                        col.Item().Text($"Documento: {cliente.NroDocumento ?? "-"}");
                        col.Item().Text($"Dirección: {cliente.DomicilioComercial ?? cliente.DomicilioParticular ?? "-"}");
                        col.Item().Text($"Teléfono: {cliente.Telefono ?? cliente.TelefonoMovil ?? "-"}");
                        col.Item().Text($"Email: {cliente.EMail ?? "-"}");
                    });
                });

                column.Item().PaddingVertical(15);

                // Tabla de detalles
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);    // Cantidad
                        columns.RelativeColumn(3);     // Descripción
                        columns.ConstantColumn(80);    // Precio Unit.
                        columns.ConstantColumn(80);    // Subtotal
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Cant.").Bold();
                        header.Cell().Element(CellStyle).Text("Descripción").Bold();
                        header.Cell().Element(CellStyle).AlignRight().Text("Precio Unit.").Bold();
                        header.Cell().Element(CellStyle).AlignRight().Text("Subtotal").Bold();

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5)
                                .BorderBottom(1).BorderColor(Colors.Black);
                        }
                    });

                    // Detalles
                    foreach (var detalle in detalles)
                    {
                        table.Cell().Element(CellStyle).Text(detalle.Cantidad.ToString()).FontSize(sizeImporte);
                        table.Cell().Element(CellStyle).Text($"Artículo ID: {detalle.Articulo_Id}").FontSize(sizeImporte);
                        table.Cell().Element(CellStyle).AlignRight().Text($"${detalle.Precio_Unitario:N2}").FontSize(sizeImporte);
                        table.Cell().Element(CellStyle).AlignRight().Text($"${detalle.Subtotal:N2}").FontSize(sizeImporte);

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                        }
                    }
                });
            });
        }

        private void ComposeFooter(IContainer container, Comprobante comprobante, Cliente cliente, List<ComprobanteDetalle> detalles, bool mostrarGastosEnvio = true)
        {
            container.Column(column =>
            {
                // Totales y QR
                column.Item().Row(row =>
                {
                    // QR Code
                    row.ConstantItem(120).Column(col =>
                    {
                        if (!string.IsNullOrEmpty(comprobante.CAE))
                        {
                            try
                            {
                                var qrBytes = _qrService.GenerarQrBytes(comprobante, cliente);
                                col.Item().Image(qrBytes);
                                col.Item().AlignCenter().Text("Escanear QR para verificar").FontSize(7);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "No se pudo generar QR en PDF");
                                col.Item().Text("QR no disponible").FontSize(8);
                            }
                        }
                    });

                    row.RelativeItem();

                    // Totales
                    row.ConstantItem(200).Column(col =>
                    {
                        var subtotal = detalles.Sum(d => d.Subtotal);
                        var iva = comprobante.Total - subtotal;

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:").FontSize(8);
                            r.ConstantItem(80).AlignRight().Text($"${subtotal:N2}").FontSize(sizeImporte);
                        });

                        if (iva > 0)
                        {
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text("IVA (21%):");
                                r.ConstantItem(80).AlignRight().Text($"${iva:N2}");
                            });
                        }

                        col.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL:").FontSize(8).Bold();
                            r.ConstantItem(80).AlignRight().Text($"${comprobante.Total:N2}").FontSize(sizeImporte).Bold();
                        });

                        // Contra Entrega
                        if (comprobante.ContraEntrega.HasValue && comprobante.ContraEntrega.Value > 0)
                        {
                            col.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("Contra Entrega:");
                                r.ConstantItem(80).AlignRight().Text($"${comprobante.ContraEntrega.Value:N2}");
                            });
                        }

                        // Cuotas restantes
                        if (comprobante.Cuotas.HasValue && comprobante.Cuotas.Value > 0 && comprobante.ValorCuota.HasValue && comprobante.ValorCuota.Value > 0)
                        {
                            col.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text($"Resta pagar {comprobante.Cuotas.Value} Cuotas de ${comprobante.ValorCuota.Value:N2}").FontSize(sizeCuotas);
                            });
                        }

                        // Gastos de Envío solo en primera hoja
                        if (mostrarGastosEnvio && comprobante.GastosEnvio.HasValue && comprobante.GastosEnvio.Value > 0)
                        {
                            col.Item().PaddingTop(10).Border(1).Background(Colors.Grey.Lighten4).Padding(5).Row(r =>
                            {
                                r.RelativeItem().Text("Valor Envío:").FontSize(10).Bold();
                                r.ConstantItem(80).AlignRight().Text($"$ {comprobante.GastosEnvio.Value:N2}").FontSize(10).Bold();
                            });
                        }
                    });
                });

                // Paginación
                column.Item().PaddingTop(10).AlignCenter().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        }

        private string DeterminarTipoComprobanteTexto(string? categoriaIva)
        {
            return categoriaIva?.ToUpper() switch
            {
                "RESPONSABLE INSCRIPTO" => "A",
                "MONOTRIBUTO" => "B",
                "CONSUMIDOR FINAL" => "C",
                "EXENTO" => "B",
                _ => "C"
            };
        }

        private string ObtenerTipoComprobanteCompleto(string? tipoComprobante)
        {
            return tipoComprobante?.ToUpper() switch
            {
                "FC" => "FACTURA",
                "NC" => "NOTA DE CRÉDITO",
                "PRE" => "PRESUPUESTO",
                _ => "COMPROBANTE"
            };
        }

        private string ObtenerLetraComprobante(string? tipoComprobante)
        {
            return tipoComprobante?.ToUpper() switch
            {
                "FC" => "C",      // Factura C
                "NC" => "C",      // Nota de Crédito C
                "PRE" => "X",     // Presupuesto (sin letra oficial)
                _ => "C"
            };
        }

        public byte[] GenerarComprobanteCompletoConCupones(Comprobante comprobante, Cliente cliente, List<ComprobanteDetalle> detalles, List<Cuota> cuotas)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    // ===== COMPROBANTE POR TRIPLICADO (3 páginas) =====
                    for (int copia = 1; copia <= 3; copia++)
                    {
                        var esPrimeraHoja = copia == 1;
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(2, Unit.Centimetre);
                            page.DefaultTextStyle(x => x.FontSize(10));

                            page.Header().Element(h => ComposeHeader(h, comprobante));
                            page.Content().Element(c => ComposeContent(c, comprobante, cliente, detalles));
                            page.Footer().Element(f => ComposeFooter(f, comprobante, cliente, detalles, esPrimeraHoja));
                        });
                    }

                    // ===== CUPONES DE CUOTAS =====
                    if (cuotas.Any())
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(30);
                            page.DefaultTextStyle(x => x.FontSize(10));

                            page.Header().Element(c => ComposeCuponesHeader(c, cliente, comprobante, cuotas));
                            page.Content().Element(c => ComposeCuponesContent(c, cliente, comprobante, cuotas));
                        });
                    }
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF completo del comprobante con cupones");
                throw;
            }
        }

        private void ComposeCuponesHeader(IContainer container, Cliente cliente, Comprobante comprobante, List<Cuota> cuotas)
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
                var primerCuota = cuotas.FirstOrDefault();
                var montoCuota = primerCuota?.Importe ?? 0;

                column.Item().Border(1).Padding(5)
                    .Text($"Anticipo: ${anticipo:N2} - {cuotas.Count} Cuotas de ${montoCuota:N2}")
                    .FontSize(12).Bold().AlignCenter();
            });
        }

        private void ComposeCuponesContent(IContainer container, Cliente cliente, Comprobante comprobante, List<Cuota> cuotas)
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

                int numeroCuota = 1;
                foreach (var cuota in cuotas)
                {
                    RenderCuponCell(table, cliente, comprobante, cuota, numeroCuota);
                    numeroCuota++;
                }
            });
        }

        private void RenderCuponCell(TableDescriptor table, Cliente cliente, Comprobante comprobante, Cuota cuota, int numeroCuota)
        {
            table.Cell().Padding(5).Element(container =>
            {
                container.Border(1).Padding(5).Column(c =>
                {
                    c.Spacing(3);

                    // Encabezado del cupón
                    c.Item().Border(1).Background(Colors.Grey.Lighten3)
                        .Padding(5).Text("BOOKSTORE APP").FontSize(11).Bold().AlignCenter();

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
                            col.Item().Text($"Estado: {cuota.Estado}").FontSize(8);
                        });

                        row.RelativeItem(30).Column(col =>
                        {
                            col.Item().AlignRight().Padding(4)
                                .Text($"{numeroCuota}").FontSize(16).Bold();
                        });
                    });

                    // Información adicional
                    c.Item().PaddingTop(3).Text($"Factura: {comprobante.NumeroComprobante}").FontSize(7);
                });
            });
        }
    }
}
