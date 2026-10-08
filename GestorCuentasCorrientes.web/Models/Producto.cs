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

        [StringLength(30, ErrorMessage = "El código no puede exceder 30 caracteres")]
        [Display(Name = "Código")]
        public string? Codigo { get; set; }

        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        [StringLength(20)]
        [Display(Name = "Unidad de medida")]
        public string UnidadMedida { get; set; } = "Unidad";

        // Unidades disponibles en los formularios (static: EF la ignora, no es una columna)
        public static readonly string[] Unidades =
            { "Unidad", "Metro", "m²", "m³", "Kilo", "Litro", "Caja", "Pack" };

        // Navegación: en qué presupuestos se usó este producto
        public ICollection<PresupuestoDetalle> PresupuestoDetalles { get; set; }
            = new List<PresupuestoDetalle>();

        [Required(ErrorMessage = "El precio unitario es obligatorio")]
        [Range(0.01, 999999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }
}