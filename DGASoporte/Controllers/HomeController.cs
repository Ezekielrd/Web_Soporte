using DGASoporte.Data;
using DGASoporte.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Dynamic;

namespace DGASoporte.Controllers
{
    public class HomeController : Controller
    {
        private readonly DGADbContext _context;

        public HomeController(DGADbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            dynamic model = new ExpandoObject();

            // Cargar todas las tareas con sus relaciones
            model.Tareas = await _context.Tareas
                .Include(t => t.Estado)
                .Include(t => t.Prioridad)
                .Include(t => t.Categoria)
                .Include(t => t.Unidad)
                .Include(t => t.Tecnico)
                    .ThenInclude(te => te.Usuario)
                .Include(t => t.Tecnico)
                    .ThenInclude(te => te.Nivel)
                .Where(t => !t.Archivada)
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();

            // Cargar técnicos con sus relaciones
            model.Tecnicos = await _context.Tecnicos
                .Include(t => t.Usuario)
                .Include(t => t.Nivel)
                .Include(t => t.TareasAsignadas)
                .OrderBy(t => t.Usuario.NombreCompleto)
                .ToListAsync();

            // Cargar catálogos
            model.Estados = await _context.Estados
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            model.Prioridades = await _context.Prioridades
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            model.Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            model.Unidades = await _context.Unidades
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            model.Niveles = await _context.Niveles
                .OrderBy(n => n.Nombre)
                .ToListAsync();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> TestConexion()
        {
            bool canConnect = await _context.Database.CanConnectAsync();

            if (canConnect)
            {
                return Content("Conexión a la base de datos exitosa.");
            }
            else
            {
                return Content("No se pudo conectar a la base de datos.");
            }
        }
    }
}
