using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParqueaderoWEB.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Documento { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Usuario")] // Mapea la propiedad UsuarioNombre a la columna 'Usuario' de la tabla
        public string UsuarioNombre { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Contrasena { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Rol { get; set; } = "Empleado";

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaUltimoAcceso { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Propiedades de navegación
        [NotMapped]
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
