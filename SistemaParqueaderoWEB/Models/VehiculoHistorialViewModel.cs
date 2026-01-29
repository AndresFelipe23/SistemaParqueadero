using System;
using System.Collections.Generic;

namespace SistemaParqueaderoWEB.Models
{
    public class VehiculoHistorialViewModel
    {
        public Vehiculo? Vehiculo { get; set; }
        public List<RegistroParqueo> Registros { get; set; } = new();

        public List<Vehiculo> Vehiculos { get; set; } = new();

        public decimal TotalRecaudado { get; set; }
        public int TotalVisitas { get; set; }
        public TimeSpan TiempoTotalParqueado { get; set; }

        public string PlacaBuscada { get; set; } = string.Empty;
    }
}

