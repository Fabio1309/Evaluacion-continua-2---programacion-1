// Controllers/InmueblesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EC2_PROGRA1.Data;
using EC2_PROGRA1.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // <--- AÑADE ESTA LÍNEA
using Microsoft.AspNetCore.Authorization; // <--- AÑADE ESTA LÍNEA

namespace EC2_PROGRA1.Controllers
{
    public class InmueblesController : Controller

    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; // <--- AÑADE ESTA LÍNEA


        // Inyectamos el DbContext para poder acceder a la base de datos
        public InmueblesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Inmuebles/Catalogo
        // Este método recibirá los parámetros del formulario de filtro
        public async Task<IActionResult> Catalogo(string? ciudad, TipoInmueble? tipo, decimal? precioMin, decimal? precioMax, int? dormitorios)
        {
            // VALIDACIONES SERVER-SIDE
            if (precioMin.HasValue && precioMin < 0)
            {
                ModelState.AddModelError("precioMin", "El precio mínimo no puede ser negativo.");
            }
            if (precioMax.HasValue && precioMax < 0)
            {
                ModelState.AddModelError("precioMax", "El precio máximo no puede ser negativo.");
            }
            if (precioMin.HasValue && precioMax.HasValue && precioMin > precioMax)
            {
                ModelState.AddModelError("", "El rango de precios no es coherente (mínimo > máximo).");
            }

            // Construcción de la consulta base (Query)
            var inmueblesQuery = _context.Inmuebles
                                         .Where(i => i.Activo) // Solo inmuebles activos
                                         .AsQueryable();

            // Aplicar filtros a la consulta
            if (!string.IsNullOrEmpty(ciudad))
            {
                inmueblesQuery = inmueblesQuery.Where(i => i.Ciudad.Contains(ciudad));
            }
            if (tipo.HasValue)
            {
                inmueblesQuery = inmueblesQuery.Where(i => i.Tipo == tipo.Value);
            }
            if (precioMin.HasValue)
            {
                inmueblesQuery = inmueblesQuery.Where(i => i.Precio >= precioMin.Value);
            }
            if (precioMax.HasValue)
            {
                inmueblesQuery = inmueblesQuery.Where(i => i.Precio <= precioMax.Value);
            }
            if (dormitorios.HasValue)
            {
                inmueblesQuery = inmueblesQuery.Where(i => i.Dormitorios >= dormitorios.Value);
            }

            // Guardamos los filtros en ViewBag para mostrarlos de nuevo en la vista
            ViewBag.Ciudad = ciudad;
            ViewBag.Tipo = tipo;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.Dormitorios = dormitorios;

            // Ejecutamos la consulta y la pasamos a la vista
            var inmuebles = await inmueblesQuery.ToListAsync();
            return View(inmuebles);
        }
        // Añade este método DENTRO de la clase InmueblesController
        // GET: Inmuebles/Detalle/5
        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inmueble = await _context.Inmuebles.FirstOrDefaultAsync(m => m.Id == id);

            if (inmueble == null)
            {
                return NotFound();
            }

            // Lógica para verificar si hay una reserva activa
            var reservaActiva = await _context.Reservas
                .AnyAsync(r => r.InmuebleId == id && r.FechaExpiracion > DateTime.UtcNow);

            ViewBag.TieneReservaActiva = reservaActiva;

            return View(inmueble);
        }

        // AÑADE este método dentro de InmueblesController.cs
        [HttpPost]
        [Authorize] // Solo usuarios autenticados pueden ejecutar esta acción
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgendarVisita(int inmuebleId, DateTime fechaInicio, DateTime fechaFin, string? notas)
        {
            // --- VALIDACIONES SERVER-SIDE ---
            // 1. Fechas coherentes
            if (fechaInicio >= fechaFin)
            {
                TempData["ErrorMessage"] = "La fecha de inicio debe ser anterior a la fecha de fin.";
                return RedirectToAction("Detalle", new { id = inmuebleId });
            }

            // 2. Visitas en horario laboral (8:00 a 19:00)
            if (fechaInicio.Hour < 8 || fechaInicio.Hour >= 19 || fechaFin.Hour < 8 || fechaFin.Hour > 19)
            {
                TempData["ErrorMessage"] = "Las visitas solo pueden agendarse en horario laboral (08:00 a 19:00).";
                return RedirectToAction("Detalle", new { id = inmuebleId });
            }

            // 3. Evitar solapamiento de visitas
            var visitaSolapada = await _context.Visitas
                .AnyAsync(v => v.InmuebleId == inmuebleId &&
                            v.Estado != EstadoVisita.Cancelada &&
                            fechaInicio < v.FechaFin && fechaFin > v.FechaInicio);

            if (visitaSolapada)
            {
                TempData["ErrorMessage"] = "Ya existe una visita agendada en ese intervalo de tiempo.";
                return RedirectToAction("Detalle", new { id = inmuebleId });
            }

            // --- Si todas las validaciones pasan ---
            var usuario = await _userManager.GetUserAsync(User);

            // AÑADE ESTA COMPROBACIÓN
            if (usuario == null)
            {
                return Challenge(); // Redirige al login si el usuario no se encuentra
            }

            var nuevaVisita = new Visita
            {
                InmuebleId = inmuebleId,
                UsuarioId = usuario.Id,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Notas = notas,
                Estado = EstadoVisita.Solicitada
            };

            _context.Visitas.Add(nuevaVisita);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "¡Visita agendada con éxito! Nos pondremos en contacto para confirmar.";
            return RedirectToAction("Detalle", new { id = inmuebleId });
        }
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservar(int inmuebleId)
        {
            // Re-validar que no exista una reserva activa por si el usuario tenía la página abierta
            var yaExisteReserva = await _context.Reservas
                .AnyAsync(r => r.InmuebleId == inmuebleId && r.FechaExpiracion > DateTime.UtcNow);

            if (yaExisteReserva)
            {
                TempData["ErrorMessage"] = "Este inmueble ya ha sido reservado por otro usuario.";
                return RedirectToAction("Detalle", new { id = inmuebleId });
            }

            var usuario = await _userManager.GetUserAsync(User);

            // AÑADE ESTA COMPROBACIÓN
            if (usuario == null)
            {
                return Challenge(); // Redirige al login si el usuario no se encuentra
            }

            var nuevaReserva = new Reserva
            {
                InmuebleId = inmuebleId,
                UsuarioId = usuario.Id,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddHours(48) // Reserva por 48h
            };

            _context.Reservas.Add(nuevaReserva);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "¡Inmueble reservado por 48 horas con éxito!";
            return RedirectToAction("Detalle", new { id = inmuebleId });
        }
    }
}