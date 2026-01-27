namespace SistemaParqueaderoWEB.Models
{
    public class ReciboViewModel
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string TipoVehiculo { get; set; } = string.Empty;
        public DateTime FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }
        public TimeSpan TiempoParqueado { get; set; }
        public decimal MontoTotal { get; set; }
        public string CodigoBarras { get; set; } = string.Empty;
    }
}
