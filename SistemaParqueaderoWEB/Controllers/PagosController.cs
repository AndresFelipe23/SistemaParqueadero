using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParqueaderoWEB.Data;
using SistemaParqueaderoWEB.Models;

namespace SistemaParqueaderoWEB.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly ParqueaderoDbContext _context;
        private readonly ILogger<PagosController> _logger;

        public PagosController(ParqueaderoDbContext context, ILogger<PagosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetUsuarioId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // GET: Pagos/RegistrarPago/{registroId}
        public async Task<IActionResult> RegistrarPago(int registroId)
        {
            var registro = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Include(r => r.Pagos)
                .FirstOrDefaultAsync(r => r.Id == registroId);

            if (registro == null)
            {
                TempData["Error"] = "Registro de parqueo no encontrado.";
                return RedirectToAction("Index", "Parqueadero");
            }

            // Verificar si ya tiene un pago completado
            var pagoCompletado = registro.Pagos.FirstOrDefault(p => p.EstadoPago == "Completado");
            if (pagoCompletado != null)
            {
                TempData["Info"] = "Este registro ya tiene un pago completado.";
                return RedirectToAction("DetallePago", new { id = pagoCompletado.Id });
            }

            var model = new PagoViewModel
            {
                RegistroParqueoId = registroId,
                RegistroParqueo = registro,
                Monto = registro.MontoFinal ?? registro.MontoTotal ?? 0
            };

            return View(model);
        }

        // POST: Pagos/RegistrarPago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPago(PagoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recargar registro
                model.RegistroParqueo = await _context.RegistrosParqueo
                    .Include(r => r.Vehiculo)
                    .FirstOrDefaultAsync(r => r.Id == model.RegistroParqueoId);
                return View(model);
            }

            var registro = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .FirstOrDefaultAsync(r => r.Id == model.RegistroParqueoId);

            if (registro == null)
            {
                TempData["Error"] = "Registro de parqueo no encontrado.";
                return RedirectToAction("Index", "Parqueadero");
            }

            // Verificar si ya tiene un pago completado
            var pagoExistente = await _context.Pagos
                .FirstOrDefaultAsync(p => p.RegistroParqueoId == model.RegistroParqueoId && p.EstadoPago == "Completado");
            
            if (pagoExistente != null)
            {
                TempData["Error"] = "Este registro ya tiene un pago completado.";
                return RedirectToAction("DetallePago", new { id = pagoExistente.Id });
            }

            var usuarioId = GetUsuarioId();

            // Crear el pago
            var pago = new Pago
            {
                RegistroParqueoId = model.RegistroParqueoId,
                Monto = model.Monto,
                MetodoPago = model.MetodoPago,
                ReferenciaPago = model.ReferenciaPago,
                EstadoPago = "Completado",
                FechaPago = DateTime.Now,
                UsuarioPagoId = usuarioId,
                Observaciones = model.Observaciones,
                FechaCreacion = DateTime.Now
            };

            _context.Pagos.Add(pago);

            // Crear movimiento de caja
            var movimientoCaja = new MovimientoCaja
            {
                TipoMovimiento = "Ingreso",
                Concepto = $"Pago de parqueo - {registro.Placa} - {model.MetodoPago}",
                Monto = model.Monto,
                MetodoPago = model.MetodoPago,
                PagoId = null, // Se actualizará después de guardar
                UsuarioId = usuarioId,
                Observaciones = model.Observaciones,
                FechaMovimiento = DateTime.Now
            };

            _context.MovimientosCaja.Add(movimientoCaja);

            try
            {
                await _context.SaveChangesAsync();
                
                // Actualizar el movimiento de caja con el ID del pago
                movimientoCaja.PagoId = pago.Id;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Pago registrado exitosamente.";
                return RedirectToAction("DetallePago", new { id = pago.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago");
                ModelState.AddModelError(string.Empty, "Error al procesar el pago. Por favor, intente nuevamente.");
                
                // Recargar registro
                model.RegistroParqueo = registro;
                return View(model);
            }
        }

        // GET: Pagos/DetallePago/{id}
        public async Task<IActionResult> DetallePago(int id)
        {
            var pago = await _context.Pagos
                .Include(p => p.RegistroParqueo)
                    .ThenInclude(r => r.Vehiculo)
                .Include(p => p.UsuarioPago)
                .Include(p => p.MovimientosCaja)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // GET: Pagos/Historial
        public async Task<IActionResult> Historial(DateTime? fechaDesde, DateTime? fechaHasta, string? metodoPago, string? estadoPago)
        {
            var query = _context.Pagos
                .Include(p => p.RegistroParqueo)
                    .ThenInclude(r => r.Vehiculo)
                .Include(p => p.UsuarioPago)
                .AsQueryable();

            if (fechaDesde.HasValue)
            {
                query = query.Where(p => p.FechaPago.Date >= fechaDesde.Value.Date);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(p => p.FechaPago.Date <= fechaHasta.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(metodoPago))
            {
                query = query.Where(p => p.MetodoPago == metodoPago);
            }

            if (!string.IsNullOrWhiteSpace(estadoPago))
            {
                query = query.Where(p => p.EstadoPago == estadoPago);
            }

            var pagos = await query
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            var model = new PagoHistorialViewModel
            {
                Pagos = pagos,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                MetodoPagoFiltro = metodoPago,
                EstadoPagoFiltro = estadoPago,
                TotalRecaudado = pagos.Where(p => p.EstadoPago == "Completado").Sum(p => p.Monto),
                TotalPagos = pagos.Count
            };

            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.MetodoPago = metodoPago;
            ViewBag.EstadoPago = estadoPago;

            return View(model);
        }

        // GET: Pagos/PagosPendientes
        public async Task<IActionResult> PagosPendientes()
        {
            var registrosSinPago = await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                .Where(r => !r.Activo && 
                           r.FechaSalida.HasValue &&
                           !r.Pagos.Any(p => p.EstadoPago == "Completado"))
                .OrderByDescending(r => r.FechaSalida)
                .ToListAsync();

            return View(registrosSinPago);
        }
    }
}
