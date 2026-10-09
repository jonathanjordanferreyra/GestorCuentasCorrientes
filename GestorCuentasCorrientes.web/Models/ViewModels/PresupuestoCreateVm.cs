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
        // Solo se usa al editar: null = presupuesto nuevo
        public int? Id { get; set; }

        [Range(1, 365, ErrorMessage = "La validez debe estar entre 1 y 365 días")]
        [Display(Name = "Validez (días)")]
        public int DiasValidez { get; set; } = 15;
    }

    public class PresupuestoDetalleVm
    {
        [Required(ErrorMessage = "Debe seleccionar un producto")]
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }

        [Range(0.01, 999999, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public decimal Cantidad { get; set; } = 1;
    }
}