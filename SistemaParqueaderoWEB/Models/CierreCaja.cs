using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParqueaderoWEB.Models
{
    public class CierreCaja
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime FechaCierre { get; set; }

        [Required]
        public DateTime FechaApertura { get; set; }

        [Required]
        public DateTime FechaCierreReal { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoInicial { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoEfectivo { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoTarjeta { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoTransferencia { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoTotal { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontoEsperado { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Diferencia { get; set; }

        [Required]
        public int TotalRegistros { get; set; } = 0;

        [Required]
        public int TotalCarros { get; set; } = 0;

        [Required]
        public int TotalMotos { get; set; } = 0;

        [Required]
        public int UsuarioId { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Abierto"; // Abierto, Cerrado, Revisado

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Propiedades de navegación
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
