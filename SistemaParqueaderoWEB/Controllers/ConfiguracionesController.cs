using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParqueaderoWEB.Data;
using SistemaParqueaderoWEB.Models;
using System.Globalization;

namespace SistemaParqueaderoWEB.Controllers
{
    [Authorize]
    public class ConfiguracionesController : Controller
    {
        private readonly ParqueaderoDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfiguracionesController> _logger;

        public ConfiguracionesController(
            ParqueaderoDbContext context,
            IConfiguration configuration,
            ILogger<ConfiguracionesController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        private int GetUsuarioId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // GET: Configuraciones/Index
        public async Task<IActionResult> Index(string? categoria)
        {
            var query = _context.Configuracion
                .Include(c => c.UsuarioActualizacion)
                .AsQueryable();

            // Filtrar por categoría si se especifica
            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(c => c.Clave.StartsWith(categoria + "."));
            }

            var configuraciones = await query
                .OrderBy(c => c.Clave)
                .ToListAsync();

            ViewBag.Categoria = categoria;
            
            // Obtener categorías cargando primero los datos y procesando en memoria
            var todasLasClaves = await _context.Configuracion
                .Select(c => c.Clave)
                .ToListAsync();
            
            ViewBag.Categorias = todasLasClaves
                .Select(c => c.Contains('.') ? c.Split(new[] { '.' }, StringSplitOptions.None)[0] : c)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return View(configuraciones);
        }

        // GET: Configuraciones/Crear
        public IActionResult Crear()
        {
            var model = new ConfiguracionViewModel();
            return View(model);
        }

        // POST: Configuraciones/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ConfiguracionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar si ya existe una configuración con esa clave
            var existe = await _context.Configuracion
                .AnyAsync(c => c.Clave == model.Clave);

            if (existe)
            {
                ModelState.AddModelError("Clave", "Ya existe una configuración con esta clave.");
                return View(model);
            }

            var configuracion = new Configuracion
            {
                Clave = model.Clave,
                Valor = model.Valor,
                Tipo = model.Tipo ?? "String",
                Descripcion = model.Descripcion,
                FechaCreacion = DateTime.Now
            };

            _context.Configuracion.Add(configuracion);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Configuración creada exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear configuración");
                ModelState.AddModelError(string.Empty, "Error al crear la configuración. Por favor, intente nuevamente.");
                return View(model);
            }
        }

        // GET: Configuraciones/Editar/{id}
        public async Task<IActionResult> Editar(int id)
        {
            var configuracion = await _context.Configuracion.FindAsync(id);

            if (configuracion == null)
            {
                return NotFound();
            }

            var model = new ConfiguracionViewModel
            {
                Id = configuracion.Id,
                Clave = configuracion.Clave,
                Valor = configuracion.Valor,
                Tipo = configuracion.Tipo,
                Descripcion = configuracion.Descripcion
            };

            return View(model);
        }

        // POST: Configuraciones/Editar/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ConfiguracionViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var configuracion = await _context.Configuracion.FindAsync(id);

            if (configuracion == null)
            {
                return NotFound();
            }

            // Verificar si la clave cambió y si ya existe otra con la nueva clave
            if (configuracion.Clave != model.Clave)
            {
                var existe = await _context.Configuracion
                    .AnyAsync(c => c.Clave == model.Clave && c.Id != id);

                if (existe)
                {
                    ModelState.AddModelError("Clave", "Ya existe una configuración con esta clave.");
                    return View(model);
                }
            }

            configuracion.Clave = model.Clave;
            configuracion.Valor = model.Valor;
            configuracion.Tipo = model.Tipo ?? "String";
            configuracion.Descripcion = model.Descripcion;
            configuracion.FechaActualizacion = DateTime.Now;
            configuracion.UsuarioActualizacionId = GetUsuarioId();

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Configuración actualizada exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar configuración");
                ModelState.AddModelError(string.Empty, "Error al actualizar la configuración. Por favor, intente nuevamente.");
                return View(model);
            }
        }

        // GET: Configuraciones/Eliminar/{id}
        public async Task<IActionResult> Eliminar(int id)
        {
            var configuracion = await _context.Configuracion
                .Include(c => c.UsuarioActualizacion)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (configuracion == null)
            {
                return NotFound();
            }

            return View(configuracion);
        }

        // POST: Configuraciones/Eliminar/{id}
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var configuracion = await _context.Configuracion.FindAsync(id);

            if (configuracion == null)
            {
                return NotFound();
            }

            _context.Configuracion.Remove(configuracion);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Configuración eliminada exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar configuración");
                TempData["Error"] = "Error al eliminar la configuración. Por favor, intente nuevamente.";
                return RedirectToAction("Eliminar", new { id });
            }
        }

        // GET: Configuraciones/Tarifas
        public async Task<IActionResult> Tarifas()
        {
            // Leer desde la base de datos
            var configs = await _context.Configuracion
                .Where(c => c.Clave.StartsWith("Tarifas."))
                .ToListAsync();

            var carroPorHora = configs.FirstOrDefault(c => c.Clave == "Tarifas.CarroPorHora");
            var motoPorHora = configs.FirstOrDefault(c => c.Clave == "Tarifas.MotoPorHora");
            var carroPorMinuto = configs.FirstOrDefault(c => c.Clave == "Tarifas.CarroPorMinuto");
            var motoPorMinuto = configs.FirstOrDefault(c => c.Clave == "Tarifas.MotoPorMinuto");

            // Valores por defecto si no existen en la BD
            const decimal DEFAULT_CARRO_HORA = 2000;
            const decimal DEFAULT_MOTO_HORA = 1000;
            const decimal DEFAULT_CARRO_MINUTO = 70;
            const decimal DEFAULT_MOTO_MINUTO = 70;

            var model = new TarifasViewModel
            {
                CarroPorHora = carroPorHora != null && decimal.TryParse(carroPorHora.Valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var ch) 
                    ? ch 
                    : DEFAULT_CARRO_HORA,
                MotoPorHora = motoPorHora != null && decimal.TryParse(motoPorHora.Valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var mh) 
                    ? mh 
                    : DEFAULT_MOTO_HORA,
                CarroPorMinuto = carroPorMinuto != null && decimal.TryParse(carroPorMinuto.Valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var cm) 
                    ? cm 
                    : DEFAULT_CARRO_MINUTO,
                MotoPorMinuto = motoPorMinuto != null && decimal.TryParse(motoPorMinuto.Valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var mm) 
                    ? mm 
                    : DEFAULT_MOTO_MINUTO
            };

            return View(model);
        }

        // POST: Configuraciones/Tarifas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tarifas(TarifasViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuarioId = GetUsuarioId();
            var ahora = DateTime.Now;

            // Actualizar o crear configuraciones de tarifas en la base de datos
            var tarifas = new[]
            {
                new { Clave = "Tarifas.CarroPorHora", Valor = model.CarroPorHora.ToString(CultureInfo.InvariantCulture) },
                new { Clave = "Tarifas.MotoPorHora", Valor = model.MotoPorHora.ToString(CultureInfo.InvariantCulture) },
                new { Clave = "Tarifas.CarroPorMinuto", Valor = model.CarroPorMinuto.ToString(CultureInfo.InvariantCulture) },
                new { Clave = "Tarifas.MotoPorMinuto", Valor = model.MotoPorMinuto.ToString(CultureInfo.InvariantCulture) }
            };

            foreach (var tarifa in tarifas)
            {
                var config = await _context.Configuracion
                    .FirstOrDefaultAsync(c => c.Clave == tarifa.Clave);

                if (config != null)
                {
                    // Actualizar existente
                    config.Valor = tarifa.Valor;
                    config.Tipo = "Decimal";
                    config.FechaActualizacion = ahora;
                    config.UsuarioActualizacionId = usuarioId;
                }
                else
                {
                    // Crear nuevo
                    config = new Configuracion
                    {
                        Clave = tarifa.Clave,
                        Valor = tarifa.Valor,
                        Tipo = "Decimal",
                        Descripcion = $"Tarifa de {tarifa.Clave.Replace("Tarifas.", "")}",
                        FechaCreacion = ahora,
                        UsuarioActualizacionId = usuarioId
                    };
                    _context.Configuracion.Add(config);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tarifas actualizadas exitosamente. Los cambios se aplicarán inmediatamente en todos los cálculos del sistema.";
                return RedirectToAction("Tarifas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar tarifas");
                ModelState.AddModelError(string.Empty, "Error al actualizar las tarifas. Por favor, intente nuevamente.");
                return View(model);
            }
        }
    }
}
