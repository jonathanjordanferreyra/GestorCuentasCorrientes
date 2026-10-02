using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        [Display(Name = "Producto")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El precio unitario es obligatorio")]
        [Range(0.01, 999999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }
}