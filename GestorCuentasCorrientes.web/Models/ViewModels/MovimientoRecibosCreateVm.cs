using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models.ViewModels
{
    /// <summary>
    /// ViewModel unificado para crear tanto Movimientos simples como Recibos.
    /// Hereda de MovimientoSimpleCreateVm y agrega List<PagoLineaVm> para Recibos.
    /// 
    /// Uso:
    /// - Movimientos simples (Factura, Nota de crédito/débito, Ajuste): Se ignora Pagos
    /// - Recibos (tipo "REC"): Se usa Pagos como líneas de pago múltiples
    /// </summary>
    public class MovimientoRecibosCreateVm : MovimientoSimpleCreateVm
    {
        /// <summary>
        /// Lista de líneas de pago para Recibos.
        /// El model binding de ASP.NET Core llena esta lista automáticamente si los nombres de input son
        /// Pagos[0].MedioPagoId, Pagos[0].Importe, Pagos[1].MedioPagoId, etc.
        /// 
        /// Para movimientos simples, esta lista se ignora en el POST.
        /// </summary>
        public List<PagoLineaVm> Pagos { get; set; } = new() { new PagoLineaVm() };
    }
}
