// Controllers/BrokerController.cs
using EC2_PROGRA1.Data;
using EC2_PROGRA1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EC2_PROGRA1.Controllers
{
    [Authorize(Roles = "Broker")] // ¡Solo usuarios con el rol "Broker" pueden acceder!
    public class BrokerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrokerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Broker
        public async Task<IActionResult> Index()
        {
            var viewModel = new BrokerDashboardViewModel
            {
                // Vista de agenda del día
                VisitasDeHoy = await _context.Visitas
                    .Include(v => v.Inmueble)
                    .Where(v => v.FechaInicio.Date == DateTime.Today && v.Estado != EstadoVisita.Cancelada)
                    .OrderBy(v => v.FechaInicio)
                    .ToListAsync(),
                
                // Lista de Reservas activas
                ReservasActivas = await _context.Reservas
                    .Include(r => r.Inmueble)
                    .Where(r => r.FechaExpiracion > DateTime.UtcNow)
                    .OrderBy(r => r.FechaExpiracion)
                    .ToListAsync()
            };

            return View(viewModel);
        }
        

        public async Task<IActionResult> Inmuebles()
        {
            var inmuebles = await _context.Inmuebles.ToListAsync();
            return View(inmuebles);
        }

        // GET: /Broker/CrearInmueble
        public IActionResult CrearInmueble()
        {
            return View();
        }

        // POST: /Broker/CrearInmueble
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearInmueble([Bind("Codigo,Titulo,Imagen,Tipo,Ciudad,Direccion,Dormitorios,Banos,MetrosCuadrados,Precio,Activo")] Inmueble inmueble)
        {
            // Validación adicional para asegurar que el código es único
            if (await _context.Inmuebles.AnyAsync(i => i.Codigo == inmueble.Codigo))
            {
                ModelState.AddModelError("Codigo", "Este código ya existe.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(inmueble);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Inmueble creado con éxito.";
                return RedirectToAction(nameof(Inmuebles));
            }
            return View(inmueble);
        }

        // GET: /Broker/EditarInmueble/5
        public async Task<IActionResult> EditarInmueble(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null)
            {
                return NotFound();
            }
            return View(inmueble);
        }

        // POST: /Broker/EditarInmueble/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarInmueble(int id, [Bind("Id,Codigo,Titulo,Imagen,Tipo,Ciudad,Direccion,Dormitorios,Banos,MetrosCuadrados,Precio,Activo")] Inmueble inmueble)
        {
            if (id != inmueble.Id)
            {
                return NotFound();
            }

            // Validación de código único (excluyendo el inmueble actual)
            if (await _context.Inmuebles.AnyAsync(i => i.Codigo == inmueble.Codigo && i.Id != inmueble.Id))
            {
                ModelState.AddModelError("Codigo", "Este código ya existe en otro inmueble.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inmueble);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Inmuebles.Any(e => e.Id == inmueble.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Inmueble actualizado con éxito.";
                return RedirectToAction(nameof(Inmuebles));
            }
            return View(inmueble);
        }


        // --- Acciones para Visitas ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarVisita(int id)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita != null)
            {
                visita.Estado = EstadoVisita.Confirmada;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Visita confirmada con éxito.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarVisita(int id)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita != null)
            {
                visita.Estado = EstadoVisita.Cancelada;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Visita cancelada.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- Acciones para Reservas ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LiberarReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Reserva liberada con éxito.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}