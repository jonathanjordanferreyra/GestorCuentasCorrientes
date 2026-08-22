using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class Comprobante
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del archivo es requerido")]
        [StringLength(255, ErrorMessage = "El nombre del archivo no puede exceder 255 caracteres")]
        [Display(Name = "Nombre del Archivo")]
        public string NombreArchivo { get; set; } = null!;

        [Required(ErrorMessage = "La ruta del archivo es requerida")]
        [StringLength(500, ErrorMessage = "La ruta del archivo no puede exceder 500 caracteres")]
        [Display(Name = "Ruta del Archivo")]
        public string RutaArchivo { get; set; } = null!;

        [StringLength(10, ErrorMessage = "El tipo de archivo no puede exceder 10 caracteres")]
        [Display(Name = "Tipo de Archivo")]
        public string? TipoArchivo { get; set; }

        [Display(Name = "Fecha de Carga")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCarga { get; set; } = DateTime.Now;

        // Clave foránea explícita
        [Display(Name = "Movimiento")]
        public int MovimientoId { get; set; }

        // Propiedad de navegación
        public Movimiento? Movimiento { get; set; }
    }
}
