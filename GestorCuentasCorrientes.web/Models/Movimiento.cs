using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class Movimiento
    {
        public int Id { get; set; }

        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(30, ErrorMessage = "El número de comprobante no puede exceder 30 caracteres")]
        [Display(Name = "Número de Comprobante")]
        public string? NumeroComprobante { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "El importe debe ser mayor a 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Fecha de Registro")]
        [DataType(DataType.DateTime)]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Display(Name = "Anulado")]
        public bool Anulado { get; set; } = false;

        // Claves foráneas explícitas
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [Display(Name = "Tipo de Movimiento")]
        public int TipoMovimientoId { get; set; }

        [Display(Name = "Usuario")]
        public string UsuarioId { get; set; } = null!;

        [Display(Name = "Movimiento Origen")]
        public int? MovimientoOrigenId { get; set; }

        // Propiedades de navegación
        public Cliente? Cliente { get; set; }
        public TipoMovimiento? TipoMovimiento { get; set; }
        public Usuario? Usuario { get; set; }

        // Auto-referencia: movimiento que da origen a este (ej: factura original de una nota de crédito)
        public Movimiento? MovimientoOrigen { get; set; }

        // Colección de movimientos originados por este
        public ICollection<Movimiento> MovimientosOriginados { get; set; } = new List<Movimiento>();

        // Navegación: un movimiento puede tener muchos pagos
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        // Navegación: un movimiento puede tener muchos comprobantes
        public ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();
    }
}
