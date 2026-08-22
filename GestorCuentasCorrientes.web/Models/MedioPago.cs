using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class MedioPago
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del medio de pago es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        [Display(Name = "Medio de Pago")]
        public string Nombre { get; set; } = null!;

        // Navegación: un medio de pago puede estar en muchos pagos
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}
