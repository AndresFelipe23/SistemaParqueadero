using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParqueaderoWEB.Data;
using SistemaParqueaderoWEB.Models;

namespace SistemaParqueaderoWEB.Controllers
{
    [Authorize]
    public class CajaController : Controller
    {
        private readonly ParqueaderoDbContext _context;
        private readonly ILogger<CajaController> _logger;

        public CajaController(ParqueaderoDbContext context, ILogger<CajaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetUsuarioId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // GET: Caja/Index - Estado actual y resumen del día
        public async Task<IActionResult> Index()
        {
            var fechaActual = DateTime.Now.Date;
            var usuarioId = GetUsuarioId();

            var model = new CierreCajaViewModel
            {
                FechaActual = fechaActual
            };

            // Verificar si hay un cierre abierto para hoy
            var cierreAbierto = await _context.CierresCaja
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => 
                    c.FechaCierre.Date == fechaActual && 
                    c.Estado == "Abierto");

            model.CajaAbierta = cierreAbierto == null;
            model.CierreActual = cierreAbierto;

            if (cierreAbierto != null)
            {
                // Si hay cierre abierto, mostrar sus datos
                model.MontoEfectivo = cierreAbierto.MontoEfectivo;
                model.MontoTarjeta = cierreAbierto.MontoTarjeta;
                model.MontoTransferencia = cierreAbierto.MontoTransferencia;
                model.MontoTotal = cierreAbierto.MontoTotal;
            }
            else
            {
                // Calcular resumen del día desde los registros cerrados
                var registrosDelDia = await _context.RegistrosParqueo
                    .Include(r => r.Vehiculo)
                    .Include(r => r.Pagos)
                    .Where(r => r.FechaSalida.HasValue && 
                                r.FechaSalida.Value.Date == fechaActual &&
                                !r.Activo)
                    .ToListAsync();

                model.RegistrosDelDia = registrosDelDia;
                model.TotalRegistros = registrosDelDia.Count;
                model.TotalCarros = registrosDelDia.Count(r => r.TipoVehiculo == TipoVehiculo.Carro);
                model.TotalMotos = registrosDelDia.Count(r => r.TipoVehiculo == TipoVehiculo.Moto);
                model.MontoEsperado = registrosDelDia.Sum(r => r.MontoFinal ?? r.MontoTotal ?? 0);

                // Calcular montos por método de pago
                var pagosDelDia = await _context.Pagos
                    .Where(p => p.FechaPago.Date == fechaActual && 
                                p.EstadoPago == "Completado")
                    .ToListAsync();

                model.MontoEfectivo = pagosDelDia
                    .Where(p => p.MetodoPago.Contains("Efectivo", StringComparison.OrdinalIgnoreCase))
                    .Sum(p => p.Monto);
                model.MontoTarjeta = pagosDelDia
                    .Where(p => p.MetodoPago.Contains("Tarjeta", StringComparison.OrdinalIgnoreCase))
                    .Sum(p => p.Monto);
                model.MontoTransferencia = pagosDelDia
                    .Where(p => p.MetodoPago.Contains("Transferencia", StringComparison.OrdinalIgnoreCase))
                    .Sum(p => p.Monto);
                model.MontoTotal = model.MontoEfectivo + model.MontoTarjeta + model.MontoTransferencia;
            }

            // Cargar historial de cierres (últimos 30 días)
            model.HistorialCierres = await _context.CierresCaja
                .Include(c => c.Usuario)
                .Where(c => c.Estado == "Cerrado")
                .OrderByDescending(c => c.FechaCierreReal)
                .Take(30)
                .ToListAsync();

            return View(model);
        }

        // GET: Caja/CerrarCaja - Formulario para cerrar caja
        public async Task<IActionResult> CerrarCaja()
        {
            var fechaActual = DateTime.Now.Date;
            var usuarioId = GetUsuarioId();

            // Verificar si ya hay un cierre abierto
            var cierreAbierto = await _context.CierresCaja
                .FirstOrDefaultAsync(c => 
                    c.FechaCierre.Date == fechaActual && 
                    c.Estado == "Abierto");

            if (cierreAbierto != null)
            {
                TempData["Error"] = "Ya existe un cierre de caja abierto para hoy. Debe cerrarlo primero.";
                return RedirectToAction("Index");
            }

            var model = new CierreCajaViewModel
            {
                FechaActual = fechaActual
            };

            // Calcular resumen del día
            var registrosDelDia = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Include(r => r.Pagos)
                .Where(r => r.FechaSalida.HasValue && 
                            r.FechaSalida.Value.Date == fechaActual &&
                            !r.Activo)
                .ToListAsync();

            model.RegistrosDelDia = registrosDelDia;
            model.TotalRegistros = registrosDelDia.Count;
            model.TotalCarros = registrosDelDia.Count(r => r.TipoVehiculo == TipoVehiculo.Carro);
            model.TotalMotos = registrosDelDia.Count(r => r.TipoVehiculo == TipoVehiculo.Moto);
            model.MontoEsperado = registrosDelDia.Sum(r => r.MontoFinal ?? r.MontoTotal ?? 0);

            // Calcular montos por método de pago desde los pagos
            var pagosDelDia = await _context.Pagos
                .Where(p => p.FechaPago.Date == fechaActual && 
                            p.EstadoPago == "Completado")
                .ToListAsync();

            model.MontoEfectivo = pagosDelDia
                .Where(p => p.MetodoPago.Contains("Efectivo", StringComparison.OrdinalIgnoreCase))
                .Sum(p => p.Monto);
            model.MontoTarjeta = pagosDelDia
                .Where(p => p.MetodoPago.Contains("Tarjeta", StringComparison.OrdinalIgnoreCase))
                .Sum(p => p.Monto);
            model.MontoTransferencia = pagosDelDia
                .Where(p => p.MetodoPago.Contains("Transferencia", StringComparison.OrdinalIgnoreCase))
                .Sum(p => p.Monto);
            model.MontoTotal = model.MontoEfectivo + model.MontoTarjeta + model.MontoTransferencia;

            // Prellenar con los montos esperados
            model.MontoEfectivoReal = model.MontoEfectivo;
            model.MontoTarjetaReal = model.MontoTarjeta;
            model.MontoTransferenciaReal = model.MontoTransferencia;

            return View(model);
        }

        // POST: Caja/CerrarCaja - Procesar cierre de caja
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarCaja(CierreCajaViewModel model)
        {
            var fechaActual = DateTime.Now.Date;
            var usuarioId = GetUsuarioId();

            // Calcular registros del día (se usa tanto para validación como para el cierre)
            var registrosDelDia = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Where(r => r.FechaSalida.HasValue && 
                            r.FechaSalida.Value.Date == fechaActual &&
                            !r.Activo)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                // Recargar datos del día en el modelo
                model.RegistrosDelDia = registrosDelDia;
                model.TotalRegistros = registrosDelDia.Count;
                model.TotalCarros = registrosDelDia.Count(r => r.TipoVehiculo == TipoVehiculo.Carro);
                model.TotalMotos = registrosDelDia.Count(r => r.TipoVehiculo == TipoVehiculo.Moto);
                model.MontoEsperado = registrosDelDia.Sum(r => r.MontoFinal ?? r.MontoTotal ?? 0);

                return View(model);
            }

            // Verificar si ya existe un cierre abierto
            var cierreExistente = await _context.CierresCaja
                .FirstOrDefaultAsync(c => 
                    c.FechaCierre.Date == fechaActual && 
                    c.Estado == "Abierto");

            if (cierreExistente != null)
            {
                TempData["Error"] = "Ya existe un cierre de caja abierto para hoy.";
                return RedirectToAction("Index");
            }

            // Calcular montos esperados (usando los registros ya cargados)

            var montoEsperado = registrosDelDia.Sum(r => r.MontoFinal ?? r.MontoTotal ?? 0);
            var montoTotalReal = model.MontoEfectivoReal + model.MontoTarjetaReal + model.MontoTransferenciaReal;
            var diferencia = montoTotalReal - montoEsperado;

            // Crear el cierre de caja
            var cierreCaja = new CierreCaja
            {
                FechaCierre = fechaActual,
                FechaApertura = fechaActual.Date.AddHours(6), // Asumir apertura a las 6 AM
                FechaCierreReal = DateTime.Now,
                MontoInicial = 0,
                MontoEfectivo = model.MontoEfectivoReal,
                MontoTarjeta = model.MontoTarjetaReal,
                MontoTransferencia = model.MontoTransferenciaReal,
                MontoTotal = montoTotalReal,
                MontoEsperado = montoEsperado,
                Diferencia = diferencia,
                TotalRegistros = registrosDelDia.Count,
                TotalCarros = registrosDelDia.Count(r => r.TipoVehiculo == TipoVehiculo.Carro),
                TotalMotos = registrosDelDia.Count(r => r.TipoVehiculo == TipoVehiculo.Moto),
                UsuarioId = usuarioId,
                Observaciones = model.Observaciones,
                Estado = "Cerrado",
                FechaCreacion = DateTime.Now
            };

            _context.CierresCaja.Add(cierreCaja);

            // Crear movimiento de caja para el cierre
            var movimientoCierre = new MovimientoCaja
            {
                TipoMovimiento = "Cierre",
                Concepto = $"Cierre de caja del día {fechaActual:dd/MM/yyyy}",
                Monto = montoTotalReal,
                UsuarioId = usuarioId,
                Observaciones = model.Observaciones,
                FechaMovimiento = DateTime.Now
            };

            _context.MovimientosCaja.Add(movimientoCierre);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Cierre de caja realizado exitosamente. Diferencia: {diferencia:C}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar caja");
                ModelState.AddModelError(string.Empty, "Error al procesar el cierre de caja. Por favor, intente nuevamente.");
                
                // Recargar datos
                model.RegistrosDelDia = registrosDelDia;
                model.TotalRegistros = registrosDelDia.Count;
                model.TotalCarros = registrosDelDia.Count(r => r.TipoVehiculo == TipoVehiculo.Carro);
                model.TotalMotos = registrosDelDia.Count(r => r.TipoVehiculo == TipoVehiculo.Moto);
                model.MontoEsperado = montoEsperado;
                
                return View(model);
            }
        }

        // GET: Caja/Historial - Historial de cierres
        public async Task<IActionResult> Historial(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var query = _context.CierresCaja
                .Include(c => c.Usuario)
                .Where(c => c.Estado == "Cerrado")
                .AsQueryable();

            if (fechaDesde.HasValue)
            {
                query = query.Where(c => c.FechaCierre.Date >= fechaDesde.Value.Date);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(c => c.FechaCierre.Date <= fechaHasta.Value.Date);
            }

            var cierres = await query
                .OrderByDescending(c => c.FechaCierreReal)
                .ToListAsync();

            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;

            return View(cierres);
        }

        // GET: Caja/DetalleCierre/{id} - Detalle de un cierre específico
        public async Task<IActionResult> DetalleCierre(int id)
        {
            var cierre = await _context.CierresCaja
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cierre == null)
            {
                return NotFound();
            }

            // Obtener registros del día del cierre
            var registros = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Include(r => r.UsuarioEntrada)
                .Include(r => r.UsuarioSalida)
                .Where(r => r.FechaSalida.HasValue && 
                            r.FechaSalida.Value.Date == cierre.FechaCierre.Date &&
                            !r.Activo)
                .OrderByDescending(r => r.FechaSalida)
                .ToListAsync();

            ViewBag.Registros = registros;
            ViewBag.Cierre = cierre;

            return View(cierre);
        }
    }
}
