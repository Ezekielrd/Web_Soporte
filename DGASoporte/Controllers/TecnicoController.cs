using DGASoporte.Data;
using DGASoporte.Infraestructura;
using DGASoporte.Models.Enumeradores;
using DGASoporte.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DGASoporte.Controllers
{
    public class TecnicoController : Controller
    {
        public readonly DGADbContext _context;
        private readonly AsignacionTareasService _asignacionService;

        public TecnicoController(DGADbContext context, AsignacionTareasService asignacionService)
        {
            _context = context;
            _asignacionService = asignacionService;

        }
        public async Task<IActionResult> Index()
        {
            int tecnicoId = User.GetRequiredUserId();

            var tareas = await _context.Tareas
                .Where(t => t.TecnicoId == tecnicoId)
              //  .OrderByDescending(t => t.Prioridad)
                .ToListAsync();

            return View(tareas);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(int id)
        {
            var tarea = await _context.Tareas
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarea == null)
            {
                TempData["Error"] = "Tarea no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // 1) Obtener el técnico actual según el usuario logueado
            var usuarioId = User.GetRequiredUserId();
            var tecnico = await _context.Tecnicos
                .FirstOrDefaultAsync(t => t.Id == usuarioId);

            if (tecnico == null)
            {
                TempData["Error"] = "No se pudo identificar al técnico.";
                return RedirectToAction(nameof(Index));
            }

            // 2) Marcar la tarea como finalizada
            tarea.Estado = EstadoT.Resuelta;
            tarea.FechaActualizacion = DateTime.Now;
            _context.Tareas.Update(tarea);
            await _context.SaveChangesAsync();

            // 3) Recalcular disponibilidad del técnico (por si ya no tiene tareas activas)
            await _asignacionService.ActualizarDisponibilidadTecnicoAsync(tecnico.Id);

            // 4) Intentar asignarle automáticamente la siguiente tarea libre
            var nuevaTarea = await _asignacionService.AsignarSiguienteTareaATecnicoAsync(tecnico.Id);

            if (nuevaTarea != null)
            {
                // 5) Como le acabamos de asignar otra, volvemos a recalcular disponibilidad
                await _asignacionService.ActualizarDisponibilidadTecnicoAsync(tecnico.Id);

                TempData["Info"] =
                    $"Has finalizado la tarea #{tarea.Id}. " +
                    $"Se te asignó automáticamente la tarea #{nuevaTarea.Id}: {nuevaTarea.Titulo}.";
            }
            else
            {
                TempData["Info"] =
                    $"Has finalizado la tarea #{tarea.Id}. " +
                    "No hay más tareas pendientes sin técnico.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
