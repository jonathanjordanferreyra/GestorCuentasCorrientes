using GestorCuentasCorrientes.web.Models;
using GestorCuentasCorrientes.web.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestorCuentasCorrientes.web.Services
{
    public class ReciboPdfService
    {
        public byte[] GenerarPdf(
            MovimientoRecibosCreateVm viewModel,
            Cliente cliente,
            Dictionary<int, string> mediosPago,
            int movimientoId)
        {
            var documento = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.DefaultTextStyle(x => x.FontSize(10));

                    // ============================
                    // ENCABEZADO
                    // ============================
                    page.Header()
                        .Column(column =>
                        {
                            column.Spacing(5);

                            column.Item()
                                .AlignCenter()
                                .Text("RECIBO")
                                .Bold()
                                .FontSize(24);

                            column.Item()
                                .AlignCenter()
                                .Text("Gestor de Cuentas Corrientes")
                                .FontSize(12);

                            column.Item()
                                .LineHorizontal(1);
                        });

                    // ============================
                    // CONTENIDO
                    // ============================
                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(12);

                            // Datos principales
                            column.Item()
                                .Text(text =>
                                {
                                    text.Span("N° de Recibo: ").Bold();
                                    text.Span(viewModel.NumeroComprobante ?? $"REC-{movimientoId}");
                                });

                            column.Item()
                                .Text(text =>
                                {
                                    text.Span("Fecha: ").Bold();
                                    text.Span(viewModel.Fecha.ToString("dd/MM/yyyy"));
                                });

                            column.Item()
                                .Text(text =>
                                {
                                    text.Span("Cliente: ").Bold();
                                    text.Span(cliente.RazonSocial);
                                });

                            if (!string.IsNullOrWhiteSpace(cliente.CuitDni))
                            {
                                column.Item()
                                    .Text(text =>
                                    {
                                        text.Span("CUIT/DNI: ").Bold();
                                        text.Span(cliente.CuitDni);
                                    });
                            }

                            column.Item().PaddingTop(10);

                            // ============================
                            // DETALLE DE PAGOS
                            // ============================
                            column.Item()
                                .Text("Detalle de pagos")
                                .Bold()
                                .FontSize(14);

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                    });

                                    // Encabezado
                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Background(Colors.Grey.Lighten2)
                                            .Padding(5)
                                            .Text("Medio de Pago")
                                            .Bold();

                                        header.Cell()
                                            .Background(Colors.Grey.Lighten2)
                                            .Padding(5)
                                            .AlignRight()
                                            .Text("Importe")
                                            .Bold();
                                    });

                                    // Líneas
                                    foreach (var linea in viewModel.Pagos)
                                    {
                                        var nombreMedio =
                                            mediosPago.GetValueOrDefault(
                                                linea.MedioPagoId,
                                                "Medio de pago");

                                        table.Cell()
                                            .Padding(5)
                                            .Text(nombreMedio);

                                        table.Cell()
                                            .Padding(5)
                                            .AlignRight()
                                            .Text(linea.Importe.ToString("C2"));
                                    }
                                });

                            // ============================
                            // TOTAL
                            // ============================
                            column.Item()
                                .PaddingTop(10)
                                .AlignRight()
                                .Text(text =>
                                {
                                    text.Span("TOTAL RECIBIDO: ")
                                        .Bold()
                                        .FontSize(14);

                                    text.Span(viewModel.Pagos.Sum(p => p.Importe)
                                        .ToString("C2"))
                                        .Bold()
                                        .FontSize(14);
                                });

                            // ============================
                            // DATOS DE CHEQUES
                            // ============================
                            var cheques = viewModel.Pagos
                                .Where(p =>
                                    !string.IsNullOrWhiteSpace(p.ChequeNumero))
                                .ToList();

                            if (cheques.Any())
                            {
                                column.Item()
                                    .PaddingTop(15)
                                    .Text("Datos de cheques")
                                    .Bold()
                                    .FontSize(14);

                                foreach (var cheque in cheques)
                                {
                                    column.Item()
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(8)
                                        .Column(chequeColumn =>
                                        {
                                            chequeColumn.Spacing(3);

                                            chequeColumn.Item()
                                                .Text(text =>
                                                {
                                                    text.Span("Número: ").Bold();
                                                    text.Span(
                                                        cheque.ChequeNumero ?? "-");
                                                });

                                            chequeColumn.Item()
                                                .Text(text =>
                                                {
                                                    text.Span("Banco: ").Bold();
                                                    text.Span(
                                                        cheque.ChequeBanco ?? "-");
                                                });

                                            chequeColumn.Item()
                                                .Text(text =>
                                                {
                                                    text.Span("Titular: ").Bold();
                                                    text.Span(
                                                        cheque.ChequeTitular ?? "-");
                                                });

                                            chequeColumn.Item()
                                                .Text(text =>
                                                {
                                                    text.Span("Fecha de emisión: ").Bold();
                                                    text.Span(
                                                        cheque.ChequeFechaEmision
                                                            ?.ToString("dd/MM/yyyy")
                                                        ?? "-");
                                                });

                                            chequeColumn.Item()
                                                .Text(text =>
                                                {
                                                    text.Span("Fecha de cobro: ").Bold();
                                                    text.Span(
                                                        cheque.ChequeFechaCobro
                                                            ?.ToString("dd/MM/yyyy")
                                                        ?? "-");
                                                });
                                        });
                                }
                            }

                            // Observaciones
                            if (!string.IsNullOrWhiteSpace(viewModel.Observaciones))
                            {
                                column.Item()
                                    .PaddingTop(15)
                                    .Text("Observaciones")
                                    .Bold();

                                column.Item()
                                    .Text(viewModel.Observaciones);
                            }

                            column.Item()
                                .PaddingTop(25)
                                .Text("Documento generado automáticamente por el sistema.")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    // ============================
                    // PIE
                    // ============================
                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                        });
                });
            });

            return documento.GeneratePdf();
        }
    }
}