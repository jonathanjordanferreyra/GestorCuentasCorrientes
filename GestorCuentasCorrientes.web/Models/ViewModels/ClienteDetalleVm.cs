namespace GestorCuentasCorrientes.web.Models.ViewModels
{
    public class ClienteDetalleVm
    {
        public Cliente Cliente { get; set; } = null!;
        public decimal SaldoActual { get; set; }
        public List<MovimientoDetalleVm> Movimientos { get; set; } = new List<MovimientoDetalleVm>();
    }
}
