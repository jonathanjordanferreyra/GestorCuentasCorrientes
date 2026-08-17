namespace GestorCuentasCorrientes.web.Models
{
    public class Comprobante
    {
        public int Id { get; set; }
        public int MovimientoId { get; set; }
        public string NombreArchivo { get; set; }
        public string RutaArchivo { get; set; }
        public string TipoArchivo { get; set; }
        public DateTime FechaCarga { get; set; }
    }
}
