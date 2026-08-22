using System.ComponentModel.DataAnnotations;

namespace GestorCuentasCorrientes.web.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La razón social es requerida")]
        [StringLength(150, ErrorMessage = "La razón social no puede exceder 150 caracteres")]
        [Display(Name = "Razón Social")]
        public string RazonSocial { get; set; } = null!;

        [StringLength(20, ErrorMessage = "El CUIT/DNI no puede exceder 20 caracteres")]
        [Display(Name = "CUIT/DNI")]
        public string? CuitDni { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [StringLength(30, ErrorMessage = "El teléfono no puede exceder 30 caracteres")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de Alta")]
        [DataType(DataType.DateTime)]
        public DateTime FechaAlta { get; set; } = DateTime.Now;

        // Clave foránea explícita
        [Display(Name = "Localidad")]
        public int LocalidadId { get; set; }

        // Propiedad de navegación
        public Localidad? Localidad { get; set; }

        // Navegación: un cliente puede tener muchos movimientos
        public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    }
}
