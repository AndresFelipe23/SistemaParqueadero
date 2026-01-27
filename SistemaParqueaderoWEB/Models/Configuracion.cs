using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParqueaderoWEB.Models
{
    public class Configuracion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Valor { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Tipo { get; set; } // String, Number, Boolean, Date

        [StringLength(500)]
        public string? Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaActualizacion { get; set; }

        public int? UsuarioActualizacionId { get; set; }

        // Propiedades de navegación
        [ForeignKey("UsuarioActualizacionId")]
        public virtual Usuario? UsuarioActualizacion { get; set; }
    }
}
