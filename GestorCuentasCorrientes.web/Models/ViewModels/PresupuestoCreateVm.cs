using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models.ViewModels
{
    public class PresupuestoCreateVm
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }
        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Now;
        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }
        public List<PresupuestoDetalleVm> Detalles { get; set; } = new();
    }
    public class PresupuestoDetalleVm
    {
        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0.01, 999999, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public decimal Cantidad { get; set; } = 1;

        [Range(0.01, 999999999.99, ErrorMessage = "El precio unitario debe ser mayor a 0")]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }
    }
}
