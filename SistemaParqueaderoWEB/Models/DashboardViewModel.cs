namespace SistemaParqueaderoWEB.Models
{
    public class DashboardViewModel
    {
        // Estadísticas generales
        public int VehiculosActivos { get; set; }
        public int CarrosActivos { get; set; }
        public int MotosActivas { get; set; }
        public decimal IngresosHoy { get; set; }
        public decimal IngresosMes { get; set; }
        public int RegistrosHoy { get; set; }
        public int PagosPendientes { get; set; }
        public decimal MontoPendiente { get; set; }

        // Estadísticas por tipo de vehículo
        public int TotalCarrosHoy { get; set; }
        public int TotalMotosHoy { get; set; }
        public decimal IngresosCarrosHoy { get; set; }
        public decimal IngresosMotosHoy { get; set; }

        // Vehículos activos recientes
        public List<VehiculoActivoViewModel> VehiculosActivosRecientes { get; set; } = new();

        // Actividades recientes
        public List<ActividadRecienteViewModel> ActividadesRecientes { get; set; } = new();

        // Datos para gráficos (últimos 7 días)
        public List<IngresoDiarioViewModel> IngresosUltimos7Dias { get; set; } = new();
    }

    public class ActividadRecienteViewModel
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Entrada" o "Salida"
        public string Placa { get; set; } = string.Empty;
        public string TipoVehiculo { get; set; } = string.Empty;
        public decimal? Monto { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class IngresoDiarioViewModel
    {
        public DateTime Fecha { get; set; }
        public decimal Ingreso { get; set; }
        public int Registros { get; set; }
    }
}
