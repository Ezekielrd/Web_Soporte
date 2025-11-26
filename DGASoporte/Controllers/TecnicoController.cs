using DGASoporte.Data;
using DGASoporte.Infraestructura;
using DGASoporte.Models;
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
                 .Include(t => t.Categoria)
                 .Include(t => t.Unidad)
                .Where(t => t.TecnicoId == tecnicoId && !t.Archivada)
                .OrderByDescending(t => t.Prioridad)
                .ToListAsync();

            return View(tareas);
        }
        [HttpGet]
        public async Task<IActionResult> Detalle(int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                TempData["Alert"] = "El identificador no es válido.";
                return RedirectToAction(nameof(Index));
            }

            var tarea = await _context.Tareas
                 .AsNoTracking()
                 .Include(x => x.Historial)
                 .Include(t => t.Categoria)
                 .Include(t => t.Usuario)
                 .Include(t => t.Unidad)
                 .Include(t => t.Tecnico)!.ThenInclude(te => te.Usuario)
                 .Include(t => t.TipoServicio)
                 .FirstOrDefaultAsync(t => t.Id == id, ct);


            // 2. Si aun así no se encontró, regresamos a Index (ya no NotFound())
            if (tarea == null)
            {
                TempData["Alert"] = $"No se encontró la tarea con Id {id}.";
                return RedirectToAction(nameof(Index));
            }
            // Si el contador está activo (EnProceso) y hay marca de inicio,
            // sumamos el tiempo transcurrido desde ese momento
            if (tarea.Estado == EstadoT.EnProceso && tarea.InicioContador.HasValue)
            {
                var transcurrido = DateTime.Now - tarea.InicioContador.Value;
                tarea.TiempoInvertido += transcurrido;
                tarea.InicioContador = DateTime.Now; // reseteamos la marca
                await _context.SaveChangesAsync();
            }

            // 3. Mapear a tu ViewModel
            var vm = new TareaFormVM
            {
                Id = tarea.Id,
                Titulo = tarea.Titulo,
                Descripcion = tarea.Descripcion,
                FechaCreacion = tarea.FechaCreacion,
                FechaLimite = tarea.FechaLimite?.ToLocalTime(),
                CategoriaId = tarea.CategoriaId,
                CategoriaNombre = tarea.Categoria?.Nombre,
                UnidadId = tarea.UnidadId,
                UnidadNombre = tarea.Unidad?.Nombre,
                TecnicoId = tarea.TecnicoId,
                TecnicoNombre = tarea.Tecnico?.Usuario.NombreCompleto,
                Estado = tarea.Estado,
                Prioridad = tarea.Prioridad,
                EstadoNombre = EnumExtension.GetDisplayName(tarea.Estado!),
                PrioridadNombre = EnumExtension.GetDisplayName(tarea.Prioridad),
                TipoServicioId = tarea.TipoServicioId,
                TipoServicioNombre = tarea.TipoServicio?.Nombre,
                TiempoInvertido = tarea.TiempoInvertido,
                Historial = tarea.Historial.OrderByDescending(h => h.FechaHora).ToList(),
                FechaInicioDiagnostico = tarea.FechaInicioDiagnostico

            };

            // 4. Soporte para AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_TecnicoDetalle", vm);
            }

            return View(vm); // Vista: Views/Tecnico/Detalle.cshtml
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarDiagnostico(int id, string diagnosticoInicial)
        {
            var t = await _context.Tareas.FindAsync(id);
            if (t == null) return NotFound();

            if (t.Estado != EstadoT.Asignado) return BadRequest();

            // Validación servidor
            if (string.IsNullOrWhiteSpace(diagnosticoInicial))
            {
                ModelState.AddModelError("DiagnosticoInicial", "El diagnóstico inicial es obligatorio.");
                return RedirectToAction(nameof(Detalle), new { id });
            }

            t.Estado = EstadoT.EnProceso;
            t.FechaActualizacion = DateTime.Now;
            t.FechaInicioDiagnostico = DateTime.Now;
            t.InicioContador = DateTime.Now;
            t.DiagnosticoInicial = diagnosticoInicial;

            await _context.SaveChangesAsync();

            // ➜ Redirige al INDEX
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pausar(int id, string motivo)
        {
            var t = await _context.Tareas.FindAsync(id);
            if (t == null) return NotFound();
            if (t.Estado != EstadoT.EnProceso) return BadRequest();

            if (string.IsNullOrWhiteSpace(motivo))
            {
                ModelState.AddModelError("motivo", "El motivo es obligatorio.");
                return RedirectToAction(nameof(Detalle), new { id });
            }

            // 1. Sumar tiempo trabajado
            if (t.InicioContador.HasValue)
            {
                t.TiempoInvertido += DateTime.Now - t.InicioContador.Value;
                t.InicioContador = null;
            }

            // 2. Cambiar estado
            t.Estado = EstadoT.Pausado;
            t.FechaActualizacion = DateTime.Now;

            // 3. Guardar comentario
            var comentario = new Comentario
            {
                Texto = $"Pausado: {motivo}",
                FechaHora = DateTime.Now,
                UsuarioId = User.GetRequiredUserId(),
                TareaId = id
            };
            _context.Comentarios.Add(comentario);

            await _context.SaveChangesAsync();

            // 4. Redirige a Index
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reanudar(int id, string motivo)
        {
            var t = await _context.Tareas.FindAsync(id);
            if (t == null) return NotFound();
            if (t.Estado != EstadoT.Pausado) return BadRequest();

            // 1. Cambiar estado
            t.Estado = EstadoT.EnProceso;
            t.FechaActualizacion = DateTime.Now;

            // 2. Reiniciar cronómetro
            t.InicioContador = DateTime.Now;

            // 3. Comentario (opcional)
            if (!string.IsNullOrWhiteSpace(motivo))
            {
                var comentario = new Comentario
                {
                    Texto = $"Reanudado: {motivo}",
                    FechaHora = DateTime.Now,
                    UsuarioId = User.GetRequiredUserId(),
                    TareaId = id
                };
                _context.Comentarios.Add(comentario);
            }

            await _context.SaveChangesAsync();

            // 4. Redirige a Index
            return RedirectToAction(nameof(Index));
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
            if (tarea.Estado != EstadoT.EnProceso) return BadRequest();

            // Sumamos último tramo
            if (tarea.InicioContador.HasValue)
            {
                tarea.TiempoInvertido += DateTime.Now - tarea.InicioContador.Value;
                tarea.InicioContador = null;
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
