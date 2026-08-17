namespace GestorCuentasCorrientes.web.Models
{
    public class Movimiento
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int TipoMovimientoId { get; set; }
        //Ver si es tipo de dato guid o string en usuarioid y ver si hacer la tabla usuario.
        public string UsuarioId { get; set; }
        public int MovimientoOrigenId { get; set; }
        public DateTime Fecha { get; set; }
        public string NumeroComprobante { get; set; }
        public decimal Importe { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Anulado { get; set; }

    }
}
