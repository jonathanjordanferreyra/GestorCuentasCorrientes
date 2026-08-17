namespace GestorCuentasCorrientes.web.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public int MovimientoId { get; set; }
        public int MedioPagoId { get; set; }
        public decimal Importe { get; set; }
    }
}
