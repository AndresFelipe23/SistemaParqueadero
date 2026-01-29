using System.ComponentModel.DataAnnotations;

namespace SistemaParqueaderoWEB.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Usuario")]
        public string UsuarioNombre { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [Display(Name = "Recordar este dispositivo")]
        public bool Recordarme { get; set; }

        public string? ReturnUrl { get; set; }
    }
}

