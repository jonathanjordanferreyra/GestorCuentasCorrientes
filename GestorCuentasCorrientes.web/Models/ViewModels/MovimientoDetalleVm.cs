namespace GestorCuentasCorrientes.web.Models.ViewModels
{
    public class MovimientoDetalleVm
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoMovimientoNombre { get; set; } = null!;
        public string? NumeroComprobante { get; set; }
        public decimal Importe { get; set; }
        public short Signo { get; set; }
        public bool Anulado { get; set; }
        public decimal SaldoAcumulado { get; set; }
        public int? ComprobanteId { get; set; }
    }
}
