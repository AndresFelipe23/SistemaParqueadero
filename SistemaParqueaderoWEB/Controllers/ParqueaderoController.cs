using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParqueaderoWEB.Data;
using SistemaParqueaderoWEB.Models;
using System.Globalization;
using System.Security.Claims;

namespace SistemaParqueaderoWEB.Controllers
{
    [Authorize]
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

        // GET: Parqueadero/Index - Página principal (Dashboard)
        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimos7Dias = hoy.AddDays(-6);

            var model = new DashboardViewModel();

            // Vehículos activos
            var vehiculosActivos = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Where(r => r.Activo)
                .ToListAsync();

            model.VehiculosActivos = vehiculosActivos.Count;
            model.CarrosActivos = vehiculosActivos.Count(r => r.TipoVehiculo == TipoVehiculo.Carro);
            model.MotosActivas = vehiculosActivos.Count(r => r.TipoVehiculo == TipoVehiculo.Moto);

            // Ingresos del día (pagos completados hoy)
            var pagosHoy = await _context.Pagos
                .Where(p => p.EstadoPago == "Completado" && p.FechaPago.Date == hoy)
                .SumAsync(p => p.Monto);

            model.IngresosHoy = pagosHoy;

            // Ingresos del mes
            var pagosMes = await _context.Pagos
                .Where(p => p.EstadoPago == "Completado" && p.FechaPago >= inicioMes)
                .SumAsync(p => p.Monto);

            model.IngresosMes = pagosMes;

            // Registros del día (entradas y salidas)
            var registrosHoy = await _context.RegistrosParqueo
                .Where(r => r.FechaEntrada.Date == hoy || (r.FechaSalida.HasValue && r.FechaSalida.Value.Date == hoy))
                .CountAsync();

            model.RegistrosHoy = registrosHoy;

            // Pagos pendientes
            var pagosPendientes = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Where(r => !r.Activo && r.FechaSalida.HasValue && 
                           !r.Pagos.Any(p => p.EstadoPago == "Completado"))
                .ToListAsync();

            model.PagosPendientes = pagosPendientes.Count;
            model.MontoPendiente = pagosPendientes
                .Where(r => r.MontoFinal.HasValue)
                .Sum(r => r.MontoFinal!.Value);

            // Estadísticas por tipo de vehículo (hoy)
            var registrosHoyList = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Include(r => r.Pagos)
                .Where(r => r.FechaSalida.HasValue && r.FechaSalida.Value.Date == hoy)
                .ToListAsync();

            model.TotalCarrosHoy = registrosHoyList.Count(r => r.TipoVehiculo == TipoVehiculo.Carro);
            model.TotalMotosHoy = registrosHoyList.Count(r => r.TipoVehiculo == TipoVehiculo.Moto);

            model.IngresosCarrosHoy = registrosHoyList
                .Where(r => r.TipoVehiculo == TipoVehiculo.Carro)
                .SelectMany(r => r.Pagos)
                .Where(p => p.EstadoPago == "Completado")
                .Sum(p => p.Monto);

            model.IngresosMotosHoy = registrosHoyList
                .Where(r => r.TipoVehiculo == TipoVehiculo.Moto)
                .SelectMany(r => r.Pagos)
                .Where(p => p.EstadoPago == "Completado")
                .Sum(p => p.Monto);

            // Vehículos activos recientes (últimos 5)
            var vehiculosActivosRecientes = vehiculosActivos
                .OrderByDescending(r => r.FechaEntrada)
                .Take(5)
                .ToList();

            foreach (var r in vehiculosActivosRecientes)
            {
                var tiempo = DateTime.Now - r.FechaEntrada;
                var monto = await CalcularMontoEstimado(r, tiempo);
                model.VehiculosActivosRecientes.Add(new VehiculoActivoViewModel
                {
                    Registro = r,
                    MontoEstimado = monto
                });
            }

            // Actividades recientes (últimas 10)
            var actividades = new List<ActividadRecienteViewModel>();

            // Entradas recientes
            var entradasRecientes = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Include(r => r.UsuarioEntrada)
                .Where(r => r.FechaEntrada >= ultimos7Dias)
                .OrderByDescending(r => r.FechaEntrada)
                .Take(10)
                .ToListAsync();

            foreach (var r in entradasRecientes)
            {
                actividades.Add(new ActividadRecienteViewModel
                {
                    Fecha = r.FechaEntrada,
                    Tipo = "Entrada",
                    Placa = r.Placa,
                    TipoVehiculo = r.TipoVehiculoTexto,
                    Usuario = r.UsuarioEntrada != null ? $"{r.UsuarioEntrada.Nombre} {r.UsuarioEntrada.Apellido}" : "N/A"
                });
            }

            // Salidas recientes
            var salidasRecientes = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Include(r => r.UsuarioSalida)
                .Include(r => r.Pagos)
                .Where(r => r.FechaSalida.HasValue && r.FechaSalida.Value >= ultimos7Dias)
                .OrderByDescending(r => r.FechaSalida)
                .Take(10)
                .ToListAsync();

            foreach (var r in salidasRecientes)
            {
                var pago = r.Pagos.FirstOrDefault(p => p.EstadoPago == "Completado");
                actividades.Add(new ActividadRecienteViewModel
                {
                    Fecha = r.FechaSalida!.Value,
                    Tipo = "Salida",
                    Placa = r.Placa,
                    TipoVehiculo = r.TipoVehiculoTexto,
                    Monto = pago?.Monto ?? r.MontoFinal,
                    Usuario = r.UsuarioSalida != null ? $"{r.UsuarioSalida.Nombre} {r.UsuarioSalida.Apellido}" : "N/A"
                });
            }

            model.ActividadesRecientes = actividades
                .OrderByDescending(a => a.Fecha)
                .Take(10)
                .ToList();

            // Ingresos últimos 7 días para gráfico
            for (int i = 6; i >= 0; i--)
            {
                var fecha = hoy.AddDays(-i);
                var ingresosDia = await _context.Pagos
                    .Where(p => p.EstadoPago == "Completado" && p.FechaPago.Date == fecha)
                    .SumAsync(p => p.Monto);

                var registrosDia = await _context.RegistrosParqueo
                    .Where(r => r.FechaSalida.HasValue && r.FechaSalida.Value.Date == fecha)
                    .CountAsync();

                model.IngresosUltimos7Dias.Add(new IngresoDiarioViewModel
                {
                    Fecha = fecha,
                    Ingreso = ingresosDia,
                    Registros = registrosDia
                });
            }

            return View(model);
        }

        // GET: Parqueadero/Entrada - Formulario de entrada
        public IActionResult Entrada()
        {
            return View();
        }

        // POST: Parqueadero/Entrada - Registrar entrada de vehículo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entrada(string placa, TipoVehiculo tipoVehiculo, string? codigoBarras, string? observacionesEntrada)
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
                Activo = true,
                ObservacionesEntrada = string.IsNullOrWhiteSpace(observacionesEntrada) ? null : observacionesEntrada.Trim()
            };

            // Asignar usuario de entrada si hay sesión
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var usuarioId))
            {
                registro.UsuarioEntradaId = usuarioId;
            }

            _context.RegistrosParqueo.Add(registro);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Vehículo {placa} registrado exitosamente";
            TempData["RegistroId"] = registro.Id;

            return RedirectToAction(nameof(ReciboEntrada), new { id = registro.Id });
        }

        // GET: Parqueadero/Salida - Formulario de salida
        public IActionResult Salida(string? placa)
        {
            ViewBag.Placa = placa?.Trim().ToUpper();
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

            // Calcular monto y actualizar datos de salida
            registro.FechaSalida = DateTime.Now;
            registro.MontoTotal = await CalcularMonto(registro);
            registro.MontoFinal = registro.MontoTotal - registro.DescuentoAplicado;
            registro.Activo = false;

            // Guardar minutos totales parqueado
            if (registro.TiempoParqueado.HasValue)
            {
                registro.TiempoParqueadoMinutos = (int)Math.Round(registro.TiempoParqueado.Value.TotalMinutes);
            }

            // Asignar usuario de salida si hay sesión
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var usuarioId))
            {
                registro.UsuarioSalidaId = usuarioId;
            }

            registro.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Salida registrada exitosamente para el vehículo {registro.Placa}";
            TempData["RegistroId"] = registro.Id;

            // Redirigir a registrar el pago
            return RedirectToAction("RegistrarPago", "Pagos", new { registroId = registro.Id });
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
            var montoEstimado = await CalcularMontoEstimado(registro, tiempoParqueado);

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
            var registros = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Where(r => r.Activo)
                .OrderByDescending(r => r.FechaEntrada)
                .ToListAsync();

            var tarifas = await ObtenerTarifasAsync();
            
            var lista = new List<VehiculoActivoViewModel>();
            foreach (var r in registros)
            {
                var tiempo = DateTime.Now - r.FechaEntrada;
                var monto = await CalcularMontoEstimado(r, tiempo);
                lista.Add(new VehiculoActivoViewModel
                {
                    Registro = r,
                    MontoEstimado = monto
                });
            }

            ViewBag.CarroPorHora = tarifas.CarroPorHora;
            ViewBag.MotoPorHora = tarifas.MotoPorHora;
            ViewBag.CarroPorMinuto = tarifas.CarroPorMinuto;
            ViewBag.MotoPorMinuto = tarifas.MotoPorMinuto;

            return View(lista);
        }

        // GET: Parqueadero/HistorialVehiculo?placa=ABC123
        public async Task<IActionResult> HistorialVehiculo(string? placa)
        {
            var model = new VehiculoHistorialViewModel
            {
                PlacaBuscada = placa?.ToUpper().Trim() ?? string.Empty
            };

            // Listar todos los vehículos para poder seleccionarlos
            model.Vehiculos = await _context.Vehiculos
                .OrderByDescending(v => v.FechaUltimaVisita ?? v.FechaPrimeraVisita ?? v.FechaCreacion)
                .ToListAsync();

            // Base: todos los registros (para poder listar historial completo)
            var query = _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Include(r => r.UsuarioEntrada)
                .Include(r => r.UsuarioSalida)
                .Include(r => r.Pagos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(placa))
            {
                placa = placa.Trim().ToUpper();

                var vehiculo = await _context.Vehiculos
                    .FirstOrDefaultAsync(v => v.Placa == placa);

                if (vehiculo == null)
                {
                    ModelState.AddModelError(string.Empty, $"No se encontró un vehículo con placa {placa}.");
                }
                else
                {
                    model.Vehiculo = vehiculo;
                    query = query.Where(r => r.Vehiculo.Placa == placa);
                }
            }

            var registros = await query
                .OrderByDescending(r => r.FechaEntrada)
                .ToListAsync();

            model.Registros = registros;
            model.TotalVisitas = registros.Count;
            model.TotalRecaudado = registros.Sum(r => r.MontoFinal ?? 0);

            var totalMinutos = registros
                .Where(r => r.TiempoParqueadoMinutos.HasValue)
                .Sum(r => r.TiempoParqueadoMinutos!.Value);
            model.TiempoTotalParqueado = TimeSpan.FromMinutes(totalMinutos);

            return View(model);
        }

        // Métodos auxiliares
        private string GenerarCodigoBarras()
        {
            return $"PARQ{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }

        // Obtener tarifas desde la base de datos
        private async Task<(decimal CarroPorHora, decimal MotoPorHora, decimal CarroPorMinuto, decimal MotoPorMinuto)> ObtenerTarifasAsync()
        {
            try
            {
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

                return (
                    CarroPorHora: carroPorHora != null && decimal.TryParse(carroPorHora.Valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var ch) 
                        ? ch 
                        : DEFAULT_CARRO_HORA,
                    MotoPorHora: motoPorHora != null && decimal.TryParse(motoPorHora.Valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var mh) 
                        ? mh 
                        : DEFAULT_MOTO_HORA,
                    CarroPorMinuto: carroPorMinuto != null && decimal.TryParse(carroPorMinuto.Valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var cm) 
                        ? cm 
                        : DEFAULT_CARRO_MINUTO,
                    MotoPorMinuto: motoPorMinuto != null && decimal.TryParse(motoPorMinuto.Valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var mm) 
                        ? mm 
                        : DEFAULT_MOTO_MINUTO
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener tarifas desde BD, usando valores por defecto");
                // Valores por defecto en caso de error
                return (
                    CarroPorHora: 2000,
                    MotoPorHora: 1000,
                    CarroPorMinuto: 70,
                    MotoPorMinuto: 70
                );
            }
        }

        private async Task<decimal> CalcularMonto(RegistroParqueo registro)
        {
            if (!registro.FechaSalida.HasValue)
            {
                return 0;
            }

            var tiempoParqueado = registro.FechaSalida.Value - registro.FechaEntrada;
            return await CalcularMontoEstimado(registro, tiempoParqueado);
        }

        private async Task<decimal> CalcularMontoEstimado(RegistroParqueo registro, TimeSpan tiempoParqueado)
        {
            var tarifas = await ObtenerTarifasAsync();
            decimal monto = 0;

            // Calcular horas completas y minutos restantes
            var horasCompletas = tiempoParqueado.Hours;
            var minutosRestantes = tiempoParqueado.Minutes;
            var totalMinutos = (int)tiempoParqueado.TotalMinutes;

            if (registro.TipoVehiculo == TipoVehiculo.Carro)
            {
                // Si es menos de una hora, cobrar solo por minutos
                if (horasCompletas < 1)
                {
                    monto = totalMinutos * tarifas.CarroPorMinuto;
                }
                else
                {
                    // Cobrar horas completas por hora + minutos restantes por minuto
                    monto = (horasCompletas * tarifas.CarroPorHora) + (minutosRestantes * tarifas.CarroPorMinuto);
                }
            }
            else // Moto
            {
                // Si es menos de una hora, cobrar solo por minutos
                if (horasCompletas < 1)
                {
                    monto = totalMinutos * tarifas.MotoPorMinuto;
                }
                else
                {
                    // Cobrar horas completas por hora + minutos restantes por minuto
                    monto = (horasCompletas * tarifas.MotoPorHora) + (minutosRestantes * tarifas.MotoPorMinuto);
                }
            }

            return Math.Round(monto, 2);
        }
    }
}
