using DGASoporte.Data;
using DGASoporte.Hubs;
using DGASoporte.Models;
using DGASoporte.Models.Enumeradores;
using DGASoporte.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;


public class TareaController : Controller
{
    private readonly DGADbContext _context;
    private readonly AsignacionTareasService _asignacionService;
    private readonly IHubContext<NotificacionesHub> _notificacionesHub;


    private const int tecnicoid = 2;

    public TareaController(DGADbContext context, AsignacionTareasService asignacionService, IHubContext<NotificacionesHub> notificacionesHub)
    {
        _context = context;
        _asignacionService = asignacionService;
        _notificacionesHub = notificacionesHub;
    }

    // GET
    [HttpGet]
    public async Task<IActionResult> Index()
    {

        dynamic model = new ExpandoObject();
        // DEBUG contadores
        ViewBag.TotalEF = await _context.Tareas
            .IgnoreQueryFilters()
            .CountAsync();

        ViewBag.NoArchivadasEF = await _context.Tareas
            .IgnoreQueryFilters()
            .CountAsync(t => !t.Archivada);

        // Cargar todas las tareas con sus relaciones
        model.Tareas = await _context.Tareas
                .IgnoreQueryFilters()
            .Include(t => t.Categoria)
            .Include(t => t.Unidad)
            .Include(t => t.TipoServicio)
            .Include(t => t.Tecnico)
            .ThenInclude(te => te.Usuario)
                .Include(t => t.Tecnico)
                .ThenInclude(te => te.Nivel)
            .Where(t => !t.Archivada)
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync();

        ViewBag.CantTareasModelo = ((List<Tarea>)model.Tareas).Count;

        // Cargar técnicos con sus relaciones
        model.Tecnicos = await _context.Tecnicos
            .Include(t => t.Usuario)
            .Include(t => t.Nivel)
            .Include(t => t.TareasAsignadas)
            .OrderBy(t => t.Usuario.NombreCompleto)
            .ToListAsync();

        model.TipoServicio = await _context.TipoIncidencias
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

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_tareas", model); // solo el contenido

        return View(model);
    }

    // GET: /Tareas/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        if (id <= 0)
        {
            TempData["Alert"] = "El Identificador no es Válido";
            return RedirectToAction(nameof(Index));
        }

        var vm = await _context.Tareas
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TareaFormVM
            {
                Id = t.Id,
                Titulo = t.Titulo,
                Descripcion = t.Descripcion,
                FechaCreacion = t.FechaCreacion,
                FechaLimite = t.FechaLimite.HasValue ? t.FechaLimite.Value.ToLocalTime() : (DateTime?)null,               
                CategoriaId = t.CategoriaId,
                CategoriaNombre = t.Categoria != null ? t.Categoria.Nombre : null,
                UnidadId = t.UnidadId,
                UnidadNombre = t.Unidad != null ? t.Unidad.Nombre : null,
                TecnicoId = t.TecnicoId,
                TecnicoNombre = t.Tecnico != null ? t.Tecnico.Usuario.NombreCompleto : null,
                Estado = t.Estado,
                Prioridad= t.Prioridad,
                EstadoNombre = EnumExtension.GetDisplayName(t.Estado!),
                PrioridadNombre = EnumExtension.GetDisplayName(t.Prioridad),
                TipoServicioId = t.TipoServicioId,
                TipoServicioNombre = t.TipoServicio != null ? t.TipoServicio.Nombre : null,
                FechaInicioDiagnostico = t.FechaInicioDiagnostico
            })
            .FirstOrDefaultAsync(ct);

        if (vm is null) return NotFound();
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TareaDetalle", vm);
        }

        return View(vm);
    }

    // GET: /Tareas/Create
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = new TareaFormVM();
        await CargarSelects(vm, ct);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TareaCrear", vm);
        }
        return View(vm);
    }
    // POST: /Tareas/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TareaFormVM vm, CancellationToken ct)
    {
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        const int ID_CATEGORIA_GENERAL = 5; // Soporte General

        // --- Validar fecha límite ---
        if (vm.FechaLimite != null && vm.FechaLimite < DateTime.Now.Date)
            ModelState.AddModelError(nameof(vm.FechaLimite), "La fecha límite no puede ser anterior a hoy.");

        // --- Validar existencia de relaciones básicas ---
        bool existeArea = await _context.Unidades.AnyAsync(a => a.Id == vm.UnidadId, ct);
        if (!existeArea)
            ModelState.AddModelError(nameof(vm.UnidadId), "El área seleccionada no existe.");

        bool existeCategoria = await _context.Categorias.AnyAsync(c => c.Id == vm.CategoriaId, ct);
        if (!existeCategoria)
            ModelState.AddModelError(nameof(vm.CategoriaId), "La categoría seleccionada no existe.");

        // --- Validar TipoServicio SOLO si la categoría NO es general ---
        if (vm.CategoriaId != ID_CATEGORIA_GENERAL)
        {
            if (!vm.TipoServicioId.HasValue)
            {
                ModelState.AddModelError(nameof(vm.TipoServicioId), "Debe elegir el Tipo de Servicio");
            }
            else
            {
                bool existeTipoServicio = await _context.TipoServicios
                    .AnyAsync(c => c.Id == vm.TipoServicioId.Value, ct);

                if (!existeTipoServicio)
                {
                    ModelState.AddModelError(nameof(vm.TipoServicioId),
                        "El tipo de servicio seleccionado no existe.");
                }
            }
        }
        else
        {
            // Categoría general → aseguramos que venga en null
            vm.TipoServicioId = null;
        }

        // --- Validación División vs Unidad ---
        if (vm.UnidadId > 0)
        {
            var unidad = await _context.Unidades.FindAsync(vm.UnidadId);
            if (unidad != null && unidad.DivisionId != null && vm.DivisionId == null)
            {
                ModelState.AddModelError(nameof(vm.DivisionId), "Debe elegir la división correspondiente.");
            }
        }

        // --- Si hay errores, recargar combos y volver a la vista/partial ---
        if (!ModelState.IsValid)
        {
            await CargarSelects(vm, ct);

            if (isAjax)
                return PartialView("_TareaCrear", vm);

            return View(vm);
        }

        // ================== MAPE0 ENTIDAD ==================
        var entidad = new Tarea
        {
            Titulo = vm.Titulo.Trim(),
            Descripcion = vm.Descripcion.Trim(),
            FechaCreacion = DateTime.Now.ToLocalTime(),
            Prioridad = vm.Prioridad,
            CategoriaId = vm.CategoriaId,
            UnidadId = vm.UnidadId,
            FechaLimite = vm.FechaLimite,
            TipoServicioId = (vm.CategoriaId != ID_CATEGORIA_GENERAL)
                ? vm.TipoServicioId
                : null
        };

        Tecnico? tecnico = null;

        if (vm.TecnicoId.HasValue)
        {
            tecnico = await _context.Tecnicos
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.Id == vm.TecnicoId.Value, ct);

            if (tecnico == null)
            {
                ModelState.AddModelError(nameof(vm.TecnicoId), "El técnico seleccionado no existe.");
                await CargarSelects(vm, ct);

                if (isAjax)
                    return PartialView("_TareaCrear", vm);

                return View(vm);
            }

            entidad.TecnicoId = tecnico.Id;
            entidad.FechaAsignacion = DateTime.Now;
            entidad.Estado = EstadoT.Asignado;
            tecnico.Disponible = false;
        }
        else
        {
            entidad.Estado = EstadoT.Nuevo;
        }

        _context.Add(entidad);

        try
        {
            await _context.SaveChangesAsync(ct);

            if (tecnico != null)
            {
                var evento = new
                {
                    tareaId = entidad.Id,
                    titulo = entidad.Titulo ?? "(Sin título)",
                    tecnicoId = tecnico.Id,
                    tecnicoNombre = tecnico.Usuario?.NombreCompleto ?? "(Sin nombre)",
                    prioridad = entidad.Prioridad
                };

                await _notificacionesHub.Clients.All.SendAsync("TaskAssigned", evento);
            }

            TempData["Success"] = "La tarea fue creada correctamente.";

            if (isAjax)
                return Json(new { success = true });

            return RedirectToAction(nameof(Details), new { id = entidad.Id });
        }
        catch (DbUpdateException ex)
        {
            var innerExceptionMessage = ex.InnerException?.Message;
            ModelState.AddModelError(string.Empty,
                "No se pudo guardar la tarea. Verifica los datos e intenta nuevamente. Detalle: " + innerExceptionMessage);

            await CargarSelects(vm, ct);

            if (isAjax)
                return PartialView("_TareaCrear", vm);

            return View(vm);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.GetBaseException().Message}");

            await CargarSelects(vm, ct);

            if (isAjax)
                return PartialView("_TareaCrear", vm);

            return View(vm);
        }
    }


    // GET: /Tareas/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        if (id <= 0)
        {
            TempData["Alert"] = "El Identificador no es Válido";
            return RedirectToAction(nameof(Index));
        }
        var t = await _context.Tareas.FindAsync(id);
        if (t == null) return NotFound();

        var vm = await _context.Tareas
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TareaFormVM
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Descripcion = t.Descripcion,
                    Estado = t.Estado,
                    Prioridad = t.Prioridad,
                    CategoriaId = t.CategoriaId,
                    UnidadId = t.UnidadId,
                    FechaLimite = t.FechaLimite.HasValue ? t.FechaLimite.Value.ToLocalTime() : (DateTime?)null,
                    TecnicoId = t.TecnicoId,
                    TipoServicioId= t.TipoServicioId
                })
                .FirstOrDefaultAsync(ct);

        if (vm is null) return NotFound();

        await CargarSelects(vm, ct);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TareaEditar", vm);
        }
        return View(vm);
    }

    // POST: /Tareas/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TareaFormVM vm, CancellationToken ct)
    {
        // ¿La petición viene desde el modal (fetch)?
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (id != vm.Id) return BadRequest("Identificadores no coinciden.");

        //validar la fechaLimite
        if (vm.FechaLimite!= null && vm.FechaLimite < DateTime.Now.Date)
            ModelState.AddModelError(nameof(vm.FechaLimite), "La fecha límite no puede ser anterior a hoy.");

        // Validar relaciones
        var existeArea = await _context.Unidades.AnyAsync(a => a.Id == vm.UnidadId, ct);
        var existeCategoria = await _context.Categorias.AnyAsync(c => c.Id == vm.CategoriaId, ct);
        if (!existeArea) ModelState.AddModelError(nameof(vm.UnidadId), "Área no válida.");
        if (!existeCategoria) ModelState.AddModelError(nameof(vm.CategoriaId), "Categoría no válida.");

        if (!ModelState.IsValid)
        {
            await CargarSelects(vm,ct);
            if (isAjax)
            {
                return PartialView("_TareaEditar", vm);
            }
            return View(vm);
        }

        // Carga la entidad a editar
        var t = await _context.Tareas
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (t is null) return NotFound();

        // Concurrencia (si RowVersion existe en BD)
        if (t.RowVersion is not null && vm.RowVersion is not null && !t.RowVersion.SequenceEqual(vm.RowVersion))
        {
            ModelState.AddModelError(string.Empty, "La tarea fue modificada por otro usuario. Refresca la página e intenta de nuevo.");
            await CargarSelects(vm, ct);
            if (isAjax)
            {
                return PartialView("_TareaEditar", vm);
            }
            return View(vm);
        }

        // Si se asigna técnico por primera vez
        if (vm.TecnicoId.HasValue && !t?.TecnicoId.HasValue == true)
        {
            vm.FechaAsignacion = DateTime.Now;
            vm.Estado = EstadoT.Asignado;
        }
        else
        {
            vm.FechaActualizacion = DateTime.Now;
            vm.Estado = EstadoT.Escalado;
        }

        // Mapeo explícito: solo campos editables
        t.Titulo = vm.Titulo.Trim();
        t.Descripcion = vm.Descripcion.Trim();
        t.Estado = vm.Estado;
        t.Prioridad = vm.Prioridad;
        t.CategoriaId = vm.CategoriaId;
        t.UnidadId = vm.UnidadId;
        t.FechaLimite = vm.FechaLimite.HasValue ? vm.FechaLimite.Value.ToLocalTime() : (DateTime?)null;
        t.Archivada = vm.Archivada;
        t.FechaActualizacion = DateTime.Now.ToLocalTime();
        t.TecnicoId= vm.TecnicoId;
        try
        {
            await _context.SaveChangesAsync(ct);
            if (isAjax)
            {
                return Json(new { success = true });
            }
            TempData["Success"] = "La tarea fue actualizada correctamente.";
            return RedirectToAction(nameof(Details), new { id = t.Id });
        }
        catch (DbUpdateConcurrencyException)
        {
            // Si implementas RowVersion (ver abajo), aquí detectas ediciones simultáneas
            ModelState.AddModelError(string.Empty, "Otro usuario modificó este registro. Actualiza la página y vuelve a intentarlo.");
            if (isAjax)
            {
                // volvemos a pintar el card dentro del modal con el error
                return PartialView("_TareaEditar", vm);
            }
            return View(vm);
        }
        catch (DbUpdateException ex)
        {
            var innerExceptionMessage = ex.InnerException?.Message;
            ModelState.AddModelError(string.Empty, "No se pudo guardar la tarea. Verifica los datos e intenta nuevamente. Detalle: " + innerExceptionMessage); await CargarSelects(vm, ct);
            if (isAjax)
            {
                // volvemos a pintar el card dentro del modal con el error
                return PartialView("_TareaeEditar", vm);
            }
            return View(vm);
        }
    }

    // GET: /Tareas/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (id <= 0)
        {
            return NotFound();
        }
        var t = await _context.Tareas
           .Include(x => x.Unidad)
           .Include(x => x.Tecnico.Usuario)
           .FirstOrDefaultAsync(m => m.Id == id,ct);
        if (t == null) return NotFound();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TareaEliminar", t);
        }
        return View(t);
    }

    // POST: /Tarea/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        // Validar el ID
        if (id <= 0)
        {
            if (isAjax)
                return Json(new { success = false, message = "El Identificador no es Válido." });

            TempData["Alert"] = "El Identificador no es Válido.";
            return RedirectToAction(nameof(Index));
        }

        try {
             // Buscar tarea
             var t = await _context.Tareas
                    .FirstOrDefaultAsync(u => u.Id == id, ct);

            if (t == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Tarea no encontrado." });

                return NotFound();
            }
        
            _context.Remove(t);
            await _context.SaveChangesAsync(ct);
            if (isAjax)
                return Json(new { success = true, message = "Tarea eliminada correctamente" });

            TempData["Success"] = "La tarea fue eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (isAjax)
                return Json(new { success = false, message = "La tarea fue modificada o eliminada por otro usuario. Actualiza la página." });
            TempData["Error"] = "La tarea fue modificada o eliminada por otro usuario. Actualiza la página.";
            return RedirectToAction(nameof(Details));
        }
        catch (DbUpdateException)
        {
            // Posible restricción por FK (p. ej., comentarios/adjuntos)
            if (isAjax)
                return Json(new { success = false, message = "No se pudo eliminar la tarea porque existen datos relacionados" });
            TempData["Error"] = "No se pudo eliminar la tarea porque existen datos relacionados.";
            return RedirectToAction(nameof(Details));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Asignar(int id)
    {

        var Tarea = await _context.Tareas
       .Include(t => t.Categoria)
       .Include(t => t.Unidad)
       .Include(t => t.TipoServicio)
       .FirstOrDefaultAsync(t => t.Id == id);

        if (Tarea == null) return NotFound();

        var Tecnicos = await _context.Tecnicos
            .Include(t => t.Usuario)
            .Include(t => t.Nivel)
            .ToListAsync();

        var Niveles = await _context.Niveles.ToListAsync();

        var vm = new TareaAsignarVM
        {
            Tarea = Tarea,
            Tecnicos = Tecnicos,
            Niveles = Niveles
        };

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TareaAsignar", vm);
        }

        return View(vm);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Asignar(int tareaId, int tecnicoId)
    {
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        var ok = await _asignacionService.AsignarManualAsync(tareaId, tecnicoId);


        if (!ok)
            TempData["Error"] = "No se pudo asignar la tarea al técnico seleccionado.";
        else
            TempData["ok"] = "Tarea asignada correctamente.";
        if (isAjax)
        {
            return Json(new { success = true });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarAuto(int tareaId)
    {
        var ok = await _asignacionService.AsignarAutoPorTareaAsync(tareaId);

        return Json(new
        {
            success = ok,
            message = ok
                ? "Tarea asignada automáticamente a un técnico."
                : "No se pudo asignar automáticamente la tarea. Verifica que no tenga técnico y que haya técnicos disponibles."
        });
    }

    private async Task CargarSelects(TareaFormVM vm, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        vm.Categorias = await _context.Categorias
            .OrderBy(c => c.Nombre)
            .Select(c => new SelectListItem(c.Nombre, c.Id.ToString()))
            .ToListAsync(ct);

        ct.ThrowIfCancellationRequested();

        vm.TiposServicio = await _context.TipoServicios
             .OrderBy(t => t.Nombre)
             .AsNoTracking()
             .ToListAsync(ct);

        vm.MapTipoDesc = await _context.TipoServicios
            .ToDictionaryAsync(t => t.Id, t => t.Descripcion!, ct);

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
            .Include(t => t.Usuario)   // Para acceder a Usuario
            .OrderBy(t => t.Usuario.NombreCompleto)
            .Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Usuario.NombreCompleto
            })
            .ToListAsync();

    }
    // POST: /Tareas/Archivar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Archivar(int id)
    {
        var t = await _context.Tareas.FindAsync(id);
        if (t == null) return NotFound();

        t.Archivada = true;
        t.FechaActualizacion = DateTime.Now.ToLocalTime();
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    // AJAX: Eliminar tarea
    [HttpPost]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null)
            {
                return Json(new { success = false, message = "Tarea no encontrada." });
            }

            _context.Remove(tarea);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Tarea eliminada exitosamente." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error al eliminar: {ex.Message}" });
        }
    }

    // AJAX: Archivar/Desarchivar tarea
    [HttpPost]
    public async Task<IActionResult> ToggleArchivo(int id)
    {
        try
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null)
            {
                return Json(new { success = false, message = "Tarea no encontrada." });
            }

            tarea.Archivada = !tarea.Archivada;
            tarea.FechaActualizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                archivada = tarea.Archivada,
                message = tarea.Archivada ? "Tarea archivada." : "Tarea desarchivada."
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    // AJAX: Obtener estadísticas
    [HttpGet]
    public async Task<IActionResult> GetEstadisticas()
    {
        var total = await _context.Tareas.CountAsync(t => !t.Archivada);
        var vencidas = await _context.Tareas.CountAsync(t =>
            !t.Archivada && // No archivadas
            t.FechaLimite.HasValue && // Tiene una fecha límite asignada
            t.FechaLimite.Value.Date < DateTime.Now.Date // La fecha límite es menor que hoy
        );
        var porEstado = await _context.Tareas
            .Where(t => !t.Archivada)
          //  .GroupBy(t => t.Estado.Nombre)
          //  .Select(g => new { estado = g.Key, cantidad = g.Count() })
            .ToListAsync();

        return Json(new { total, vencidas, porEstado });
    }

    private bool TareaExists(int id)
    {
        return _context.Tareas.Any(e => e.Id == id);
    }

    private async Task CargarListasDesplegables(Tarea? tarea = null)
    {
     //   ViewData["EstadoId"] = new SelectList(await _context.Estados.ToListAsync(), "Id", "Nombre", tarea?.EstadoId);
       // ViewData["PrioridadId"] = new SelectList(await _context.Prioridades.ToListAsync(), "Id", "Nombre", tarea?.PrioridadId);
        ViewData["CategoriaId"] = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nombre", tarea?.CategoriaId);
        ViewData["UnidadId"] = new SelectList(await _context.Unidades.ToListAsync(), "Id", "Nombre", tarea?.UnidadId);
        ViewData["TecnicoId"] = new SelectList(await _context.Tecnicos.ToListAsync(), "Id", "Nombre", tarea?.TecnicoId);
    }

}
