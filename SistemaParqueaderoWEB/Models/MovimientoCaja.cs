using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParqueaderoWEB.Models
{
    public class MovimientoCaja
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoMovimiento { get; set; } = string.Empty; // Ingreso, Egreso, Apertura, Cierre

        [Required]
        [StringLength(200)]
        public string Concepto { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [StringLength(50)]
        public string? MetodoPago { get; set; }

        public int? PagoId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        // Propiedades de navegación
        [ForeignKey("PagoId")]
        public virtual Pago? Pago { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
