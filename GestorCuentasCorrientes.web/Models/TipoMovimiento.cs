using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class TipoMovimiento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(10, ErrorMessage = "El código no puede exceder 10 caracteres")]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = null!;

        [Display(Name = "Signo")]
        public short Signo { get; set; }

        // Navegación: un tipo de movimiento puede estar en muchos movimientos
        public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    }
}
