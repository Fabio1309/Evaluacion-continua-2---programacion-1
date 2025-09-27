// Controllers/InmueblesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EC2_PROGRA1.Data;
using EC2_PROGRA1.Models;
using System.Threading.Tasks;

namespace EC2_PROGRA1.Controllers
{
    public class InmueblesController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Inyectamos el DbContext para poder acceder a la base de datos
        public InmueblesController(ApplicationDbContext context)
        {
            _context = context;
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

            return View(inmueble);
        }
    }
}