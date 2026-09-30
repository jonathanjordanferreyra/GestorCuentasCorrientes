// Models/PresupuestoDetalle.cs
using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class PresupuestoDetalle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = null!;

        [Range(0.01, 999999, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public decimal Cantidad { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "El precio unitario debe ser mayor a 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        [Display(Name = "Presupuesto")]
        public int PresupuestoId { get; set; }

        public Presupuesto? Presupuesto { get; set; }
    }
}