using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestorCuentasCorrientes.web.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Localidad")]
        public int LocalidadId { get; set; }
        public string RazonSocial { get; set; }
        public string CuitDni { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaDeAlta { get; set; }

    }
}
