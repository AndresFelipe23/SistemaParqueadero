using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParqueaderoWEB.Data;
using SistemaParqueaderoWEB.Models;
using System.Globalization;

namespace SistemaParqueaderoWEB.Controllers
{
    public class ParqueaderoController : Controller
    {
        private readonly ParqueaderoDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ParqueaderoController> _logger;

        public ParqueaderoController(
            ParqueaderoDbContext context,
            IConfiguration configuration,
            ILogger<ParqueaderoController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // GET: Parqueadero/Index - Página principal
        public IActionResult Index()
        {
            return View();
        }

        // GET: Parqueadero/Entrada - Formulario de entrada
        public IActionResult Entrada()
        {
            return View();
        }

        // POST: Parqueadero/Entrada - Registrar entrada de vehículo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entrada(string placa, TipoVehiculo tipoVehiculo, string? codigoBarras)
        {
            if (string.IsNullOrWhiteSpace(placa))
            {
                ModelState.AddModelError("Placa", "La placa es requerida");
                return View();
            }

            // Normalizar placa (mayúsculas, sin espacios)
            placa = placa.Trim().ToUpper();

            // Verificar si hay un registro activo con esta placa
            var registroActivo = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .FirstOrDefaultAsync(r => r.Vehiculo.Placa == placa && r.Activo);

            if (registroActivo != null)
            {
                ModelState.AddModelError("Placa", $"Ya existe un vehículo con placa {placa} actualmente en el parqueadero");
                return View();
            }

            // Buscar o crear vehículo
            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(v => v.Placa == placa);

            if (vehiculo == null)
            {
                vehiculo = new Vehiculo
                {
                    Placa = placa,
                    TipoVehiculo = tipoVehiculo,
                    FechaPrimeraVisita = DateTime.Now,
                    FechaUltimaVisita = DateTime.Now,
                    TotalVisitas = 1
                };
                _context.Vehiculos.Add(vehiculo);
                await _context.SaveChangesAsync();
            }
            else
            {
                vehiculo.TotalVisitas++;
                vehiculo.FechaUltimaVisita = DateTime.Now;
                if (vehiculo.TipoVehiculo != tipoVehiculo)
                {
                    vehiculo.TipoVehiculo = tipoVehiculo;
                }
                await _context.SaveChangesAsync();
            }

            // Generar código de barras si no se proporciona
            if (string.IsNullOrWhiteSpace(codigoBarras))
            {
                codigoBarras = GenerarCodigoBarras();
            }

            var registro = new RegistroParqueo
            {
                VehiculoId = vehiculo.Id,
                CodigoBarras = codigoBarras,
                FechaEntrada = DateTime.Now,
                Activo = true
            };

            _context.RegistrosParqueo.Add(registro);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Vehículo {placa} registrado exitosamente";
            TempData["RegistroId"] = registro.Id;

            return RedirectToAction(nameof(ReciboEntrada), new { id = registro.Id });
        }

        // GET: Parqueadero/Salida - Formulario de salida
        public IActionResult Salida()
        {
            return View();
        }

        // POST: Parqueadero/Salida - Registrar salida de vehículo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Salida(string placa, string? codigoBarras)
        {
            if (string.IsNullOrWhiteSpace(placa) && string.IsNullOrWhiteSpace(codigoBarras))
            {
                ModelState.AddModelError("", "Debe proporcionar la placa o el código de barras");
                return View();
            }

            RegistroParqueo? registro = null;

            if (!string.IsNullOrWhiteSpace(codigoBarras))
            {
                registro = await _context.RegistrosParqueo
                    .Include(r => r.Vehiculo)
                    .FirstOrDefaultAsync(r => r.CodigoBarras == codigoBarras && r.Activo);
            }
            else if (!string.IsNullOrWhiteSpace(placa))
            {
                placa = placa.Trim().ToUpper();
                registro = await _context.RegistrosParqueo
                    .Include(r => r.Vehiculo)
                    .FirstOrDefaultAsync(r => r.Vehiculo.Placa == placa && r.Activo);
            }

            if (registro == null)
            {
                ModelState.AddModelError("", "No se encontró un vehículo activo con los datos proporcionados");
                return View();
            }

            // Calcular monto
            registro.FechaSalida = DateTime.Now;
            registro.MontoTotal = CalcularMonto(registro);
            registro.MontoFinal = registro.MontoTotal - registro.DescuentoAplicado;
            registro.Activo = false;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Salida registrada exitosamente para el vehículo {registro.Placa}";
            TempData["RegistroId"] = registro.Id;

            return RedirectToAction(nameof(ReciboSalida), new { id = registro.Id });
        }

        // GET: Parqueadero/ReciboEntrada
        public async Task<IActionResult> ReciboEntrada(int id)
        {
            var registro = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .FirstOrDefaultAsync(r => r.Id == id);
            
            if (registro == null)
            {
                return NotFound();
            }

            var recibo = new ReciboViewModel
            {
                Id = registro.Id,
                Placa = registro.Placa,
                TipoVehiculo = registro.TipoVehiculoTexto,
                FechaEntrada = registro.FechaEntrada,
                CodigoBarras = registro.CodigoBarras ?? string.Empty
            };

            return View(recibo);
        }

        // GET: Parqueadero/ReciboSalida
        public async Task<IActionResult> ReciboSalida(int id)
        {
            var registro = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .FirstOrDefaultAsync(r => r.Id == id);
            
            if (registro == null || !registro.FechaSalida.HasValue)
            {
                return NotFound();
            }

            var recibo = new ReciboViewModel
            {
                Id = registro.Id,
                Placa = registro.Placa,
                TipoVehiculo = registro.TipoVehiculoTexto,
                FechaEntrada = registro.FechaEntrada,
                FechaSalida = registro.FechaSalida,
                TiempoParqueado = registro.TiempoParqueado ?? TimeSpan.Zero,
                MontoTotal = registro.MontoTotal ?? 0,
                CodigoBarras = registro.CodigoBarras ?? string.Empty
            };

            return View(recibo);
        }

        // GET: Parqueadero/Consultar - Consultar vehículo por placa o código
        [HttpGet]
        public async Task<IActionResult> Consultar(string? placa, string? codigoBarras)
        {
            if (string.IsNullOrWhiteSpace(placa) && string.IsNullOrWhiteSpace(codigoBarras))
            {
                return Json(new { success = false, message = "Debe proporcionar la placa o el código de barras" });
            }

            RegistroParqueo? registro = null;

            if (!string.IsNullOrWhiteSpace(codigoBarras))
            {
                registro = await _context.RegistrosParqueo
                    .Include(r => r.Vehiculo)
                    .FirstOrDefaultAsync(r => r.CodigoBarras == codigoBarras && r.Activo);
            }
            else if (!string.IsNullOrWhiteSpace(placa))
            {
                placa = placa.Trim().ToUpper();
                registro = await _context.RegistrosParqueo
                    .Include(r => r.Vehiculo)
                    .FirstOrDefaultAsync(r => r.Vehiculo.Placa == placa && r.Activo);
            }

            if (registro == null)
            {
                return Json(new { success = false, message = "No se encontró un vehículo activo" });
            }

            var tiempoParqueado = DateTime.Now - registro.FechaEntrada;
            var montoEstimado = CalcularMontoEstimado(registro, tiempoParqueado);

            return Json(new
            {
                success = true,
                registro = new
                {
                    id = registro.Id,
                    placa = registro.Placa,
                    tipoVehiculo = registro.TipoVehiculoTexto,
                    fechaEntrada = registro.FechaEntrada.ToString("yyyy-MM-dd HH:mm:ss"),
                    tiempoParqueado = $"{tiempoParqueado.Hours}h {tiempoParqueado.Minutes}m",
                    montoEstimado = montoEstimado.ToString("C", new CultureInfo("es-CO"))
                }
            });
        }

        // GET: Parqueadero/VehiculosActivos - Lista de vehículos activos
        public async Task<IActionResult> VehiculosActivos()
        {
            var vehiculos = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Where(r => r.Activo)
                .OrderByDescending(r => r.FechaEntrada)
                .ToListAsync();

            return View(vehiculos);
        }

        // Métodos auxiliares
        private string GenerarCodigoBarras()
        {
            return $"PARQ{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }

        private decimal CalcularMonto(RegistroParqueo registro)
        {
            if (!registro.FechaSalida.HasValue)
            {
                return 0;
            }

            var tiempoParqueado = registro.FechaSalida.Value - registro.FechaEntrada;
            return CalcularMontoEstimado(registro, tiempoParqueado);
        }

        private decimal CalcularMontoEstimado(RegistroParqueo registro, TimeSpan tiempoParqueado)
        {
            var tarifas = _configuration.GetSection("Tarifas");
            decimal monto = 0;

            if (registro.TipoVehiculo == TipoVehiculo.Carro)
            {
                var horas = (decimal)tiempoParqueado.TotalHours;
                var tarifaPorHora = tarifas.GetValue<decimal>("CarroPorHora");
                var tarifaPorMinuto = tarifas.GetValue<decimal>("CarroPorMinuto");

                // Si es menos de una hora, cobrar por minutos
                if (horas < 1)
                {
                    var minutos = (decimal)tiempoParqueado.TotalMinutes;
                    monto = minutos * tarifaPorMinuto;
                }
                else
                {
                    // Redondear hacia arriba las horas
                    var horasRedondeadas = Math.Ceiling(horas);
                    monto = horasRedondeadas * tarifaPorHora;
                }
            }
            else // Moto
            {
                var horas = (decimal)tiempoParqueado.TotalHours;
                var tarifaPorHora = tarifas.GetValue<decimal>("MotoPorHora");
                var tarifaPorMinuto = tarifas.GetValue<decimal>("MotoPorMinuto");

                if (horas < 1)
                {
                    var minutos = (decimal)tiempoParqueado.TotalMinutes;
                    monto = minutos * tarifaPorMinuto;
                }
                else
                {
                    var horasRedondeadas = Math.Ceiling(horas);
                    monto = horasRedondeadas * tarifaPorHora;
                }
            }

            return Math.Round(monto, 2);
        }
    }
}
