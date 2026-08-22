using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class Pago
    {
        public int Id { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "El importe debe ser mayor a 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        // Claves foráneas explícitas
        [Display(Name = "Movimiento")]
        public int MovimientoId { get; set; }

        [Display(Name = "Medio de Pago")]
        public int MedioPagoId { get; set; }

        // Propiedades de navegación
        public Movimiento? Movimiento { get; set; }
        public MedioPago? MedioPago { get; set; }

        // Navegación: un pago puede tener un cheque (relación 1:1)
        public Cheque? Cheque { get; set; }
    }
}
