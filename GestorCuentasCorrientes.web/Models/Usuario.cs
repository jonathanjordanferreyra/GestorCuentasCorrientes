using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class Usuario : IdentityUser
    {

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
