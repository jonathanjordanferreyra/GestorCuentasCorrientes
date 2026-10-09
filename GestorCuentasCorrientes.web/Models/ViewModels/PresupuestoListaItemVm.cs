namespace GestorCuentasCorrientes.web.Models.ViewModels
{
    public class PresupuestoListaItemVm
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Cliente { get; set; } = "";
        public string Estado { get; set; } = "";
        public decimal Total { get; set; }
    }
}