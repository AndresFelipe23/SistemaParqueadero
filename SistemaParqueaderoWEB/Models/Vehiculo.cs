using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParqueaderoWEB.Models
{
    public class Vehiculo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Placa { get; set; } = string.Empty;

        [Required]
        public TipoVehiculo TipoVehiculo { get; set; }

        [StringLength(50)]
        public string? Marca { get; set; }

        [StringLength(50)]
        public string? Modelo { get; set; }

        [StringLength(30)]
        public string? Color { get; set; }

        [StringLength(200)]
        public string? PropietarioNombre { get; set; }

        [StringLength(20)]
        public string? PropietarioDocumento { get; set; }

        [StringLength(20)]
        public string? PropietarioTelefono { get; set; }

        public bool VehiculoFrecuente { get; set; } = false;

        public int TotalVisitas { get; set; } = 0;

        public DateTime? FechaPrimeraVisita { get; set; }

        public DateTime? FechaUltimaVisita { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaActualizacion { get; set; }

        // Propiedades de navegación
        public virtual ICollection<RegistroParqueo> RegistrosParqueo { get; set; } = new List<RegistroParqueo>();

        // Propiedades calculadas
        [NotMapped]
        public string TipoVehiculoTexto => TipoVehiculo == TipoVehiculo.Carro ? "Carro" : "Moto";
    }
}
