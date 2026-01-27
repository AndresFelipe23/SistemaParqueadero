using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParqueaderoWEB.Models
{
    public class Tarifa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public TipoVehiculo TipoVehiculo { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string TipoCobro { get; set; } = string.Empty; // PorHora, PorMinuto, Fijo

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        public int? TiempoMinimo { get; set; } // Minutos mínimos

        public int? TiempoMaximo { get; set; } // Minutos máximos

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DescuentoPorcentaje { get; set; } = 0;

        public bool Activo { get; set; } = true;

        public DateTime FechaInicio { get; set; } = DateTime.Now;

        public DateTime? FechaFin { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public int? UsuarioCreacionId { get; set; }

        // Propiedades de navegación
        [ForeignKey("UsuarioCreacionId")]
        public virtual Usuario? UsuarioCreacion { get; set; }
    }
}
