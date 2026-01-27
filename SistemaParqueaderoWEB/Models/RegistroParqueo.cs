using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParqueaderoWEB.Models
{
    public class RegistroParqueo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehiculoId { get; set; }

        [Required]
        [StringLength(50)]
        public string CodigoBarras { get; set; } = string.Empty;

        [Required]
        public DateTime FechaEntrada { get; set; }

        public DateTime? FechaSalida { get; set; }

        public int? TiempoParqueadoMinutos { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontoTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DescuentoAplicado { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontoFinal { get; set; }

        public bool Activo { get; set; } = true;

        public int? UsuarioEntradaId { get; set; }

        public int? UsuarioSalidaId { get; set; }

        [StringLength(500)]
        public string? ObservacionesEntrada { get; set; }

        [StringLength(500)]
        public string? ObservacionesSalida { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaActualizacion { get; set; }

        // Propiedades de navegación
        [ForeignKey("VehiculoId")]
        public virtual Vehiculo Vehiculo { get; set; } = null!;

        [ForeignKey("UsuarioEntradaId")]
        public virtual Usuario? UsuarioEntrada { get; set; }

        [ForeignKey("UsuarioSalidaId")]
        public virtual Usuario? UsuarioSalida { get; set; }

        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        // Propiedades calculadas
        [NotMapped]
        public TimeSpan? TiempoParqueado
        {
            get
            {
                if (FechaSalida.HasValue)
                {
                    return FechaSalida.Value - FechaEntrada;
                }
                return DateTime.Now - FechaEntrada;
            }
        }

        [NotMapped]
        public string Placa => Vehiculo?.Placa ?? string.Empty;

        [NotMapped]
        public TipoVehiculo TipoVehiculo => Vehiculo?.TipoVehiculo ?? TipoVehiculo.Carro;

        [NotMapped]
        public string TipoVehiculoTexto => Vehiculo?.TipoVehiculoTexto ?? "Carro";
    }
}
