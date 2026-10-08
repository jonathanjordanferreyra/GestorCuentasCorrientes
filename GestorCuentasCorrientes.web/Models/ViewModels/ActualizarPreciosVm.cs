using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models.ViewModels
{
    public class ActualizarPreciosVm
    {
        [Required(ErrorMessage = "Ingresá un porcentaje")]
        [Range(-90, 500, ErrorMessage = "El porcentaje debe estar entre -90 y 500")]
        [Display(Name = "Porcentaje de variación")]
        public int Porcentaje { get; set; }

        [Display(Name = "Aplicar solo a productos activos")]
        public bool SoloActivos { get; set; } = true;

        // Se completa solo al pedir la vista previa
        public List<PrecioPreviewVm> Vista { get; set; } = new();
    }

    public class PrecioPreviewVm
    {
        public string? Codigo { get; set; }
        public string Nombre { get; set; } = null!;
        public decimal PrecioActual { get; set; }
        public decimal PrecioNuevo { get; set; }
    }
}