using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models.ViewModels
{
    public class PagoLineaVm
    {
        [Required(ErrorMessage = "El medio de pago es obligatorio")]
        [Display(Name = "Medio de Pago")]
        public int MedioPagoId { get; set; }

        [Required(ErrorMessage = "El importe es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El importe debe ser mayor a 0")]
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        // Solo se completan si el medio de pago elegido es Cheque o E-cheque
        [Display(Name = "Número de Cheque")]
        public string? ChequeNumero { get; set; }

        [Display(Name = "Banco")]
        public string? ChequeBanco { get; set; }

        [Display(Name = "Titular")]
        public string? ChequeTitular { get; set; }

        [Display(Name = "Fecha de Emisión")]
        [DataType(DataType.Date)]
        public DateTime? ChequeFechaEmision { get; set; }

        [Display(Name = "Fecha de Cobro")]
        [DataType(DataType.Date)]
        public DateTime? ChequeFechaCobro { get; set; }
    }
    public class ReciboCreateVm
    {
        public int ClienteId { get; set; }

        [BindNever]
        [Display(Name = "Cliente")]
        public string? ClienteNombre { get; set; }

        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(30, ErrorMessage = "El número de comprobante no puede exceder 30 caracteres")]
        [Display(Name = "Número de Comprobante")]
        public string? NumeroComprobante { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Archivo Comprobante")]
        public IFormFile? ArchivoComprobante { get; set; }

        // Lista de tamaño variable: 1 pago, 2, 5, los que hagan falta
        public List<PagoLineaVm> Pagos { get; set; } = new() { new PagoLineaVm() };
    }
}
