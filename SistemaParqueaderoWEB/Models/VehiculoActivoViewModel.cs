using System;

namespace SistemaParqueaderoWEB.Models
{
    public class VehiculoActivoViewModel
    {
        public RegistroParqueo Registro { get; set; } = null!;
        public decimal MontoEstimado { get; set; }
    }
}

