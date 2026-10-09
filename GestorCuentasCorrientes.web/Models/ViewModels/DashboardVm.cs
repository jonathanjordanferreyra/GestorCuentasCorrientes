namespace GestorCuentasCorrientes.web.Models.ViewModels
{
    public class DashboardVm
    {
        // false = visitante sin sesión: no se carga ningún dato del negocio
        public bool MostrarResumen { get; set; }

        public int PendientesCantidad { get; set; }
        public decimal PendientesMonto { get; set; }
        public int PorVencerCantidad { get; set; }

        public int VencidosCantidad { get; set; }
        public decimal VencidosMonto { get; set; }

        public int AceptadosMesCantidad { get; set; }
        public decimal AceptadosMesMonto { get; set; }

        public int SinMovimientoCantidad { get; set; }
        public decimal SinMovimientoMonto { get; set; }

        public List<PresupuestoListaItemVm> Ultimos { get; set; } = new();
    }
}