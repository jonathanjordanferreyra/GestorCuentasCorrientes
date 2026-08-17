namespace GestorCuentasCorrientes.web.Models
{
    public class Cheque
    {
        public int Id { get; set; }
        public int PagoId { get; set; }
        public string Numero { get; set; }
        public string Banco { get; set; }
        public string Titular { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaCobro { get; set; }
        public string Estado { get; set; }







    }
}
