using BookstoreAPI.Models;
using BookstoreAPI.Models.Afip;
using BookstoreAPI.Services.Afip;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BookstoreAPI.Services.Pdf
{
    public class ComprobantePdfService : IComprobantePdfService
    {
        private readonly IAfipConfigProvider _configProvider;
        private readonly IAfipQrService _qrService;
        private readonly ILogger<ComprobantePdfService> _logger;
        private int sizeImporte = 8;
        private int sizeCuotas = 12;
        private string _cuit = string.Empty;

        public ComprobantePdfService(
            IAfipConfigProvider configProvider,
            IAfipQrService qrService,
            ILogger<ComprobantePdfService> logger)
        {
            _configProvider = configProvider;
            _qrService = qrService;
            _logger = logger;

            // Configurar licencia de QuestPDF (Community para uso no comercial)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerarPdf(Comprobante comprobante, Cliente cliente, List<ComprobanteDetalle> detalles)
        {
            try
            {
                var afipConfig = _configProvider.GetConfigAsync().GetAwaiter().GetResult();
                _cuit = afipConfig.CUIT;
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
                    // Columna izquierda - Logo y datos empresa
                    row.RelativeItem(5).Column(column =>
                    {
                        column.Item().AlignCenter().PaddingBottom(12).Width(100).Image("./Images/LiberLogo.png");
                        column.Item().PaddingTop(3).Text("de Roberto José Passarelli y Marcos E. Passarelli S.H.").FontSize(8);
                        column.Item().PaddingTop(5).Text("Av. Asamblea 1442 P. 7 Dto. 20 - C.P.: C1406HVR - CABA").FontSize(8);
                        column.Item().Text("Cel: 011 55012902 Marcos").FontSize(8);
                        column.Item().Text("Cel: 01135772183 Roberto").FontSize(8);
                        column.Item().PaddingTop(3).Text("IVA EXENTO").FontSize(8);
                    });

                    // Columna central - Letra del comprobante y tipo
                    row.RelativeItem(2).AlignTop().Column(column =>
                    {
                        column.Item().AlignCenter().Border(1).Background(Colors.Grey.Lighten3).Padding(10)
                            .Text(letraComprobante).FontSize(20).Bold().AlignCenter();
                        column.Item().PaddingTop(3).AlignCenter().Text(tipoTexto).FontSize(9).Bold();
                    });

                    // Columna derecha - Datos del comprobante
                    row.RelativeItem(5).AlignTop().PaddingHorizontal(10).Column(column =>
                    {
                        column.Item().Text("DATOS DEL COMPROBANTE").FontSize(12).Bold();
                        column.Item().PaddingTop(5).Text($"Número: {comprobante.NumeroComprobante}");
                        column.Item().Text($"Fecha: {comprobante.Fecha:dd/MM/yyyy}");

                        if (!string.IsNullOrEmpty(comprobante.CAE))
                        {
                            column.Item().Text($"CAE: {comprobante.CAE}");
                            column.Item().Text($"Vto. CAE: {comprobante.VTO:dd/MM/yyyy}");
                        }
                    });
                });

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

                // Datos del cliente
                column.Item().Column(col =>
                {
                    col.Item().Text("DATOS DEL CLIENTE").FontSize(12).Bold();
                    col.Item().PaddingTop(5).Text($"Nombre: {cliente.Nombre}");
                    col.Item().Text($"Documento: {cliente.NroDocumento ?? "-"}");
                    col.Item().Text($"Dirección: {FormatDireccionCompleta(cliente)}");
                    col.Item().Text($"Teléfono: {cliente.Telefono ?? cliente.TelefonoMovil ?? "-"}");
                    col.Item().Text($"Email: {cliente.EMail ?? "-"}");
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

        private string FormatDireccionCompleta(Cliente cliente)
        {
            var partes = new List<string>();

            var domicilio = cliente.DomicilioComercial ?? cliente.DomicilioParticular;
            if (!string.IsNullOrEmpty(domicilio))
                partes.Add(domicilio);
            if (!string.IsNullOrEmpty(cliente.CodigoPostal))
                partes.Add(cliente.CodigoPostal);
            if (!string.IsNullOrEmpty(cliente.Localidad))
                partes.Add(cliente.Localidad);
            if (!string.IsNullOrEmpty(cliente.ProvinciaDescripcion))
                partes.Add(cliente.ProvinciaDescripcion);

            return partes.Count > 0 ? string.Join(" - ", partes) : "-";
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

        public byte[] GenerarLotePdf(List<(Comprobante comprobante, Cliente cliente, List<ComprobanteDetalle> detalles)> lote)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    foreach (var item in lote)
                    {
                        for (int copia = 1; copia <= 3; copia++)
                        {
                            var esPrimeraHoja = copia == 1;
                            container.Page(page =>
                            {
                                page.Size(PageSizes.A4);
                                page.Margin(2, Unit.Centimetre);
                                page.DefaultTextStyle(x => x.FontSize(10));

                                page.Header().Element(h => ComposeHeader(h, item.comprobante));
                                page.Content().Element(c => ComposeContent(c, item.comprobante, item.cliente, item.detalles));
                                page.Footer().Element(f => ComposeFooter(f, item.comprobante, item.cliente, item.detalles, esPrimeraHoja));
                            });
                        }
                    }
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de lote de comprobantes");
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
