using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models.ViewModels
{
    public class MovimientoSimpleCreateVm
    {
        public int ClienteId { get; set; }

        [BindNever]
        [Display(Name = "Cliente")]
        public string? ClienteNombre { get; set; } 

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
        [Display(Name = "Tipo de Movimiento")]
        public int TipoMovimientoId { get; set; }

        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(30, ErrorMessage = "El número de comprobante no puede exceder 30 caracteres")]
        [Display(Name = "Número de Comprobante")]
        public string? NumeroComprobante { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El importe debe ser mayor a 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Archivo Comprobante")]
        [DataType(DataType.Upload)]
        public IFormFile? ArchivoComprobante { get; set; }
    }
}
