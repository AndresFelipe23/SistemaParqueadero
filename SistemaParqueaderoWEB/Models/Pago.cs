using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParqueaderoWEB.Models
{
    public class Pago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RegistroParqueoId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        [StringLength(50)]
        public string MetodoPago { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ReferenciaPago { get; set; }

        [Required]
        [StringLength(20)]
        public string EstadoPago { get; set; } = "Completado";

        public DateTime FechaPago { get; set; } = DateTime.Now;

        public int? UsuarioPagoId { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [ForeignKey("RegistroParqueoId")]
        public virtual RegistroParqueo RegistroParqueo { get; set; } = null!;

        [ForeignKey("UsuarioPagoId")]
        public virtual Usuario? UsuarioPago { get; set; }

        public virtual ICollection<MovimientoCaja> MovimientosCaja { get; set; } = new List<MovimientoCaja>();
    }
}
