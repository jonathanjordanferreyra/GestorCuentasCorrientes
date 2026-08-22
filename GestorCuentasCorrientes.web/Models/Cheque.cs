using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class Cheque
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de cheque es requerido")]
        [StringLength(20, ErrorMessage = "El número no puede exceder 20 caracteres")]
        [Display(Name = "Número de Cheque")]
        public string Numero { get; set; } = null!;

        [StringLength(100, ErrorMessage = "El banco no puede exceder 100 caracteres")]
        [Display(Name = "Banco")]
        public string? Banco { get; set; }

        [StringLength(150, ErrorMessage = "El titular no puede exceder 150 caracteres")]
        [Display(Name = "Titular")]
        public string? Titular { get; set; }

        [Display(Name = "Fecha de Emisión")]
        [DataType(DataType.Date)]
        public DateTime FechaEmision { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de Cobro")]
        [DataType(DataType.Date)]
        public DateTime FechaCobro { get; set; } = DateTime.Now;

        [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "EnCartera";

        // Clave foránea explícita (1:1 con Pago)
        [Display(Name = "Pago")]
        public int PagoId { get; set; }

        // Propiedad de navegación
        public Pago? Pago { get; set; }
    }
}
