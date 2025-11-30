using DGASoporte.Data;
using DGASoporte.Models;
using DGASoporte.Models.Enumeradores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Dynamic;

namespace DGASoporte.Controllers
{
    [Authorize(Roles = "Admin")]  
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
               // .Include(t => t.Estado)
               // .Include(t => t.Prioridad)
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

            model.Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            model.Unidades = await _context.Unidades
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            model.Niveles = await _context.Niveles
                .OrderBy(n => n.Nombre)
                .ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_tareas", model); // solo el contenido

            return View();
        }
        private async Task CargarCombosAsync(TareaFormVM vm, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            vm.Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                })
                .ToListAsync();

            ct.ThrowIfCancellationRequested();

            vm.Divisiones = await _context.Divisiones
                 .OrderBy(d => d.Nombre)
                 .Select(d => new SelectListItem
                 {
                     Value = d.Id.ToString(),
                     Text = d.Nombre
                 })
                 .ToListAsync(ct);

            ct.ThrowIfCancellationRequested();

            // TODAS las unidades (algunas tendrán DivisionId, otras no)
            vm.Unidades = await _context.Unidades
                .OrderBy(u => u.Nombre)
                .AsNoTracking()
                .ToListAsync(ct);
            ct.ThrowIfCancellationRequested();

            vm.Tecnicos = await _context.Tecnicos
                .OrderBy(t => t.Usuario.NombreCompleto)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Usuario.NombreCompleto
                })
                .ToListAsync();
        }
       
    }
}
