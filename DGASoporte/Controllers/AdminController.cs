using DGASoporte.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;

namespace TuProyecto.Controllers
{
    public class AdminTareasController : Controller
    {
        private readonly DGADbContext _context;

        public AdminTareasController(DGADbContext context)
        {
            _context = context;
        }

        // GET: AdminTareas
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

        // POST: AdminTareas/AsignarTarea
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarTarea(int tareaId, int tecnicoId)
        {
            try
            {
                var tarea = await _context.Tareas.FindAsync(tareaId);

                if (tarea == null)
                {
                    return Json(new { success = false, message = "Tarea no encontrada" });
                }

                tarea.TecnicoId = tecnicoId;
                tarea.FechaAsignacion = DateTime.Now;
                tarea.FechaActualizacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Tarea asignada correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: AdminTareas/EstadisticasTecnico/{id}
        public async Task<IActionResult> EstadisticasTecnico(int id)
        {
            var tareas = await _context.Tareas
                .Where(t => t.TecnicoId == id && !t.Archivada)
                .Include(t => t.Estado)
                .Include(t => t.Prioridad)
                .ToListAsync();

            var estadisticas = new
            {
                TotalTareas = tareas.Count,
                TareasCompletadas = tareas.Count(t => t.Estado?.Nombre?.ToLower() == "completado"),
                TareasPendientes = tareas.Count(t => t.Estado?.Nombre?.ToLower() == "pendiente"),
                TareasEnProceso = tareas.Count(t => t.Estado?.Nombre?.ToLower() == "proceso"),
                TareasVencidas = tareas.Count(t => t.Vencida),
                TareasAltaPrioridad = tareas.Count(t => t.Prioridad?.Nombre?.ToLower() == "alta")
            };

            return Json(estadisticas);
        }

        // POST: AdminTareas/ReasignarMasivo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReasignarMasivo(int tecnicoOrigenId, int tecnicoDestinoId)
        {
            try
            {
                var tareas = await _context.Tareas
                    .Where(t => t.TecnicoId == tecnicoOrigenId && !t.Archivada)
                    .ToListAsync();

                foreach (var tarea in tareas)
                {
                    tarea.TecnicoId = tecnicoDestinoId;
                    tarea.FechaActualizacion = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"{tareas.Count} tareas reasignadas correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
