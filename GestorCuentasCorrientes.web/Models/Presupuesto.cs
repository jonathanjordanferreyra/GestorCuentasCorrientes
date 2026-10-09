using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class Presupuesto
    {
        public int Id { get; set; }

        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Now;
        [Display(Name = "Válido hasta")]
        [DataType(DataType.Date)]
        public DateTime? FechaVencimiento { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // Pendiente | Aprobado | Rechazado | Anulado — mismo patrón que Cheque.Estado
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";

        [Display(Name = "Fecha de Registro")]
        [DataType(DataType.DateTime)]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [Display(Name = "Usuario")]
        public string UsuarioId { get; set; } = null!;

        public Cliente? Cliente { get; set; }
        public Usuario? Usuario { get; set; }
        public Movimiento? Movimiento { get; set; }
        public ICollection<PresupuestoDetalle> Detalles { get; set; } = new List<PresupuestoDetalle>();
    }
}
