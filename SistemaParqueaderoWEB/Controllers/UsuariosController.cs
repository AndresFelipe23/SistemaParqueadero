using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParqueaderoWEB.Data;
using SistemaParqueaderoWEB.Models;

namespace SistemaParqueaderoWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ParqueaderoDbContext _context;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(ParqueaderoDbContext context, ILogger<UsuariosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public class CrearUsuarioRequest
        {
            public string Nombre { get; set; } = string.Empty;
            public string Apellido { get; set; } = string.Empty;
            public string Documento { get; set; } = string.Empty;
            public string? Email { get; set; }
            public string? Telefono { get; set; }
            public string UsuarioNombre { get; set; } = string.Empty;
            public string Contrasena { get; set; } = string.Empty;
            public string Rol { get; set; } = "Administrador";
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema (para pruebas iniciales).
        /// </summary>
        [HttpPost]
        [AllowAnonymous] // IMPORTANTE: quítalo cuando ya tengas un admin creado
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.Documento) ||
                string.IsNullOrWhiteSpace(request.UsuarioNombre) ||
                string.IsNullOrWhiteSpace(request.Contrasena))
            {
                return BadRequest("Nombre, Apellido, Documento, UsuarioNombre y Contrasena son requeridos.");
            }

            // Verificar duplicados
            var existeUsuario = await _context.Usuarios
                .AnyAsync(u => u.UsuarioNombre == request.UsuarioNombre || u.Documento == request.Documento);

            if (existeUsuario)
            {
                return Conflict("Ya existe un usuario con ese nombre de usuario o documento.");
            }

            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Documento = request.Documento,
                Email = request.Email,
                Telefono = request.Telefono,
                UsuarioNombre = request.UsuarioNombre,
                // Por ahora texto plano; más adelante se debe almacenar hasheada
                Contrasena = request.Contrasena,
                Rol = string.IsNullOrWhiteSpace(request.Rol) ? "Administrador" : request.Rol,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CrearUsuario), new { id = usuario.Id }, new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Documento,
                usuario.UsuarioNombre,
                usuario.Rol,
                usuario.Activo
            });
        }
    }
}

