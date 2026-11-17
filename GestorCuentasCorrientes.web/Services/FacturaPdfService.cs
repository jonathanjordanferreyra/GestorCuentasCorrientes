using GestorCuentasCorrientes.web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestorCuentasCorrientes.web.Services
{
    public class FacturaPdfService
    {
        public byte[] GenerarPdf(Movimiento movimiento)
        {
            if (movimiento?.Cliente == null)
                throw new ArgumentException("El movimiento y su cliente son requeridos.");

            var cliente = movimiento.Cliente;
            var presupuesto = movimiento.Presupuesto;
            var detalles = presupuesto?.Detalles?.ToList() ?? new List<PresupuestoDetalle>();
            var total = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            // Paleta de colores
            const string colorPrincipal = "#1F3A5F";
            const string colorAcento = "#0D6EFD";
            const string colorTextoSuave = "#6C757D";
            const string colorBorde = "#DEE2E6";
            const string colorFondoSuave = "#F5F7FA";

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.DefaultTextStyle(x =>
                        x.FontSize(10).FontColor("#212529"));

                    // ENCABEZADO
                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Column(col =>
                                        {
                                            col.Item()
                                                .Text("Gestor de Cuentas Corrientes")
                                                .FontSize(16)
                                                .Bold()
                                                .FontColor(colorPrincipal);

                                            col.Item()
                                                .PaddingTop(2)
                                                .Text("Administración de cuentas corrientes y gestión comercial")
                                                .FontSize(9)
                                                .FontColor(colorTextoSuave);
                                        });

                                    row.ConstantItem(190)
                                        .Background(colorPrincipal)
                                        .Padding(10)
                                        .Column(col =>
                                        {
                                            col.Item()
                                                .AlignRight()
                                                .Text("FACTURA")
                                                .FontSize(10)
                                                .Bold()
                                                .FontColor("#BFD4F2");

                                            col.Item()
                                                .AlignRight()
                                                .Text($"N.° {movimiento.Id}")
                                                .FontSize(20)
                                                .Bold()
                                                .FontColor(Colors.White);
                                        });
                                });

                            column.Item()
                                .PaddingTop(12)
                                .LineHorizontal(2)
                                .LineColor(colorAcento);
                        });

                    // CONTENIDO
                    page.Content()
                        .PaddingTop(20)
                        .Column(column =>
                        {
                            column.Spacing(12);

                            // Cliente y fecha
                            column.Item()
                                .Row(row =>
                                {
                                    row.Spacing(12);

                                    row.RelativeItem()
                                        .Background(colorFondoSuave)
                                        .Border(1)
                                        .BorderColor(colorBorde)
                                        .Padding(10)
                                        .Column(col =>
                                        {
                                            col.Item()
                                                .Text("CLIENTE")
                                                .FontSize(8)
                                                .Bold()
                                                .FontColor(colorTextoSuave);

                                            col.Item()
                                                .PaddingTop(3)
                                                .Text(cliente.RazonSocial ?? "Sin cliente")
                                                .FontSize(12)
                                                .Bold();
                                        });

                                    row.RelativeItem()
                                        .Background(colorFondoSuave)
                                        .Border(1)
                                        .BorderColor(colorBorde)
                                        .Padding(10)
                                        .Column(col =>
                                        {
                                            col.Item()
                                                .Text("FECHA")
                                                .FontSize(8)
                                                .Bold()
                                                .FontColor(colorTextoSuave);

                                            col.Item()
                                                .PaddingTop(3)
                                                .Text(movimiento.Fecha.ToString("dd/MM/yyyy"))
                                                .FontSize(12)
                                                .Bold();
                                        });

                                    row.RelativeItem()
                                        .Background(colorFondoSuave)
                                        .Border(1)
                                        .BorderColor(colorBorde)
                                        .Padding(10)
                                        .Column(col =>
                                        {
                                            col.Item()
                                                .Text("COMPROBANTE")
                                                .FontSize(8)
                                                .Bold()
                                                .FontColor(colorTextoSuave);

                                            col.Item()
                                                .PaddingTop(3)
                                                .Text(movimiento.NumeroComprobante ?? "—")
                                                .FontSize(12)
                                                .Bold();
                                        });
                                });

                            // Datos del cliente adicionales
                            if (!string.IsNullOrWhiteSpace(cliente.CuitDni) || !string.IsNullOrWhiteSpace(cliente.Direccion))
                            {
                                column.Item()
                                    .Row(row =>
                                    {
                                        row.Spacing(12);

                                        if (!string.IsNullOrWhiteSpace(cliente.CuitDni))
                                        {
                                            row.RelativeItem()
                                                .Background(colorFondoSuave)
                                                .Border(1)
                                                .BorderColor(colorBorde)
                                                .Padding(10)
                                                .Column(col =>
                                                {
                                                    col.Item()
                                                        .Text("CUIT/DNI")
                                                        .FontSize(8)
                                                        .Bold()
                                                        .FontColor(colorTextoSuave);

                                                    col.Item()
                                                        .PaddingTop(3)
                                                        .Text(cliente.CuitDni)
                                                        .FontSize(11)
                                                        .Bold();
                                                });
                                        }

                                        if (!string.IsNullOrWhiteSpace(cliente.Direccion))
                                        {
                                            row.RelativeItem()
                                                .Background(colorFondoSuave)
                                                .Border(1)
                                                .BorderColor(colorBorde)
                                                .Padding(10)
                                                .Column(col =>
                                                {
                                                    col.Item()
                                                        .Text("DIRECCIÓN")
                                                        .FontSize(8)
                                                        .Bold()
                                                        .FontColor(colorTextoSuave);

                                                    col.Item()
                                                        .PaddingTop(3)
                                                        .Text(cliente.Direccion)
                                                        .FontSize(10);
                                                });
                                        }
                                    });
                            }

                            column.Item().PaddingTop(10);

                            // TABLA DE DETALLES DE PRODUCTOS
                            if (detalles.Any())
                            {
                                column.Item()
                                    .Text("Detalles de la Factura")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor(colorPrincipal);

                                column.Item()
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(4);  // Descripción
                                            columns.RelativeColumn(1.5f);  // Cantidad
                                            columns.RelativeColumn(1.5f);  // Precio Unitario
                                            columns.RelativeColumn(1.5f);  // Total
                                        });

                                        // Encabezado
                                        table.Header(header =>
                                        {
                                            header.Cell()
                                                .Background(colorPrincipal)
                                                .Padding(8)
                                                .Text("Descripción")
                                                .Bold()
                                                .FontColor(Colors.White);

                                            header.Cell()
                                                .Background(colorPrincipal)
                                                .Padding(8)
                                                .AlignCenter()
                                                .Text("Cantidad")
                                                .Bold()
                                                .FontColor(Colors.White);

                                            header.Cell()
                                                .Background(colorPrincipal)
                                                .Padding(8)
                                                .AlignRight()
                                                .Text("Precio Unit.")
                                                .Bold()
                                                .FontColor(Colors.White);

                                            header.Cell()
                                                .Background(colorPrincipal)
                                                .Padding(8)
                                                .AlignRight()
                                                .Text("Total")
                                                .Bold()
                                                .FontColor(Colors.White);
                                        });

                                        // Filas de detalles
                                        foreach (var detalle in detalles)
                                        {
                                            var totalLinea = detalle.Cantidad * detalle.PrecioUnitario;

                                            table.Cell()
                                                .Padding(6)
                                                .Text(detalle.Descripcion);

                                            table.Cell()
                                                .Padding(6)
                                                .AlignCenter()
                                                .Text(detalle.Cantidad.ToString("N2"));

                                            table.Cell()
                                                .Padding(6)
                                                .AlignRight()
                                                .Text(detalle.PrecioUnitario.ToString("C2"));

                                            table.Cell()
                                                .Padding(6)
                                                .AlignRight()
                                                .Text(totalLinea.ToString("C2"));
                                        }
                                    });
                            }

                            // TOTAL
                            column.Item()
                                .PaddingTop(15)
                                .AlignRight()
                                .Row(row =>
                                {
                                    row.RelativeItem();
                                    row.ConstantItem(250)
                                        .Column(col =>
                                        {
                                            col.Item()
                                                .Row(r =>
                                                {
                                                    r.RelativeItem()
                                                        .Text("TOTAL FACTURA:")
                                                        .Bold()
                                                        .FontSize(12);

                                                    r.ConstantItem(100)
                                                        .AlignRight()
                                                        .Text(total.ToString("C2"))
                                                        .Bold()
                                                        .FontSize(12)
                                                        .FontColor(colorAcento);
                                                });
                                        });
                                });

                            // OBSERVACIONES
                            if (!string.IsNullOrWhiteSpace(movimiento.Observaciones))
                            {
                                column.Item()
                                    .PaddingTop(15)
                                    .Column(col =>
                                    {
                                        col.Item()
                                            .Text("Observaciones:")
                                            .Bold()
                                            .FontSize(10)
                                            .FontColor(colorPrincipal);

                                        col.Item()
                                            .PaddingTop(5)
                                            .Text(movimiento.Observaciones)
                                            .FontSize(9)
                                            .FontColor(colorTextoSuave);
                                    });
                            }

                            // INFORMACIÓN DEL PRESUPUESTO
                            if (presupuesto != null)
                            {
                                column.Item()
                                    .PaddingTop(15)
                                    .Column(col =>
                                    {
                                        col.Item()
                                            .Text("Información de Presupuesto:")
                                            .Bold()
                                            .FontSize(10)
                                            .FontColor(colorPrincipal);

                                        col.Item()
                                            .PaddingTop(3)
                                            .Text(text =>
                                            {
                                                text.DefaultTextStyle(x => x.FontSize(9));
                                                text.Span("N.° Presupuesto: ").Bold();
                                                text.Span(presupuesto.Id.ToString());
                                            });

                                        col.Item()
                                            .Text(text =>
                                            {
                                                text.DefaultTextStyle(x => x.FontSize(9));
                                                text.Span("Fecha de Presupuesto: ").Bold();
                                                text.Span(presupuesto.Fecha.ToString("dd/MM/yyyy"));
                                            });

                                        if (presupuesto.FechaVencimiento.HasValue)
                                        {
                                            col.Item()
                                                .Text(text =>
                                                {
                                                    text.DefaultTextStyle(x => x.FontSize(9));
                                                    text.Span("Válido hasta: ").Bold();
                                                    text.Span(presupuesto.FechaVencimiento.Value.ToString("dd/MM/yyyy"));
                                                });
                                        }
                                    });
                            }
                        });

                    // PIE DE PÁGINA
                    page.Footer()
                        .Column(column =>
                        {
                            column.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1)
                                .LineColor(colorBorde);

                            column.Item()
                                .PaddingTop(8)
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("Generado el: ")
                                        .FontSize(8)
                                        .FontColor(colorTextoSuave);
                                    text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
                                        .FontSize(8)
                                        .FontColor(colorTextoSuave);
                                });
                        });
                });
            });

            return documento.GeneratePdf();
        }
    }
}
