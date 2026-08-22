using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class Localidad
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de localidad es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Localidad")]
        public string Nombre { get; set; } = null!;

        [StringLength(100, ErrorMessage = "La provincia no puede exceder 100 caracteres")]
        [Display(Name = "Provincia")]
        public string? Provincia { get; set; }

        // Navegación: una localidad puede tener muchos clientes
        public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    }
}
