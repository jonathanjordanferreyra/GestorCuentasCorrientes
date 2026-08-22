using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class Usuario
    {
        [StringLength(450)]
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(256, ErrorMessage = "El nombre de usuario no puede exceder 256 caracteres")]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = null!;

        // Navegación: un usuario puede tener muchos movimientos
        public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    }
}
