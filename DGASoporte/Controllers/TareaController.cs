using DGASoporte.Data;
using DGASoporte.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class TareaController : Controller
{
    private readonly DGADbContext _context;

    public TareaController(DGADbContext context)
    {
        _context = context;
    }

    // GET: /Tareas?estadoId=&prioridadId=&unidadId=&q=&incluirArchivadas=false
    [HttpGet]
    public async Task<IActionResult> Index(int? estadoId, int? prioridadId, int? unidadId,int? categoriaId, bool incluirArchivadas = false)
    {
        var qry = _context.Tareas
            .Include(t => t.Estado)
            .Include(t => t.Prioridad)
            .Include(t => t.Categoria)
            .Include(t => t.Unidad)
            .AsQueryable();

        if (!incluirArchivadas) qry = qry.Where(t => !t.Archivada);
        if (estadoId is not null) qry = qry.Where(t => t.EstadoId == estadoId);
        if (prioridadId is not null) qry = qry.Where(t => t.PrioridadId == prioridadId);
        if (unidadId is not null) qry = qry.Where(t => t.UnidadId == unidadId);
        if (unidadId is not null) qry = qry.Where(t => t.CategoriaId == categoriaId);

        ViewBag.Estados = await _context.Estados
            .OrderBy(e => e.Id)
            .Select(e => new SelectListItem(e.Nombre, e.Id.ToString()))
            .ToListAsync();

        ViewBag.Prioridades = await _context.Prioridades
            .OrderBy(p => p.Id)
            .Select(p => new SelectListItem(p.Nombre, p.Id.ToString()))
            .ToListAsync();

        ViewBag.Unidades = await _context.Unidades
            .OrderBy(u => u.Nombre)
            .Select(u => new SelectListItem(u.Nombre, u.Id.ToString()))
            .ToListAsync();

        ViewBag.Unidades = await _context.Categorias
            .OrderBy(u => u.Nombre)
            .Select(u => new SelectListItem(u.Nombre, u.Id.ToString()))
            .ToListAsync();

        ViewBag.Filtros = new { estadoId, prioridadId, unidadId,categoriaId, incluirArchivadas };

        var lista = await qry
            .OrderBy(t => t.Archivada)
            .ThenByDescending(t => t.PrioridadId)
            .ThenBy(t => t.FechaLimite)
            .ToListAsync();

        return View(lista);
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
                FechaLimite = t.FechaLimite.ToLocalTime(),

                EstadoId = t.EstadoId,
                EstadoNombre = t.Estado != null ? t.Estado.Nombre : null,

                PrioridadId = t.PrioridadId,
                PrioridadNombre = t.Prioridad != null ? t.Prioridad.Nombre : null,

                CategoriaId = t.CategoriaId,
                CategoriaNombre = t.Categoria != null ? t.Categoria.Nombre : null,

                UnidadId = t.UnidadId,
                UnidadNombre = t.Unidad != null ? t.Unidad.Nombre : null
            })
            .FirstOrDefaultAsync(ct);

        if (vm is null) return NotFound();

        return View(vm);
    }

    // GET: /Tareas/Create
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = new TareaFormVM
        {
            FechaLimite = DateTime.Today.AddDays(1).ToLocalTime()
        };
        await CargarSelects(vm, ct);
        return View(vm);
    }

    // POST: /Tareas/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TareaFormVM vm, CancellationToken ct)
    {
        //validar fecha limite
        if (vm.FechaLimite < DateTime.Now.Date)
            ModelState.AddModelError(nameof(vm.FechaLimite), "La fecha límite no puede ser anterior a hoy.");

        //validar la existencia de relaciones
        var existeEstado = await _context.Estados.AnyAsync(e => e.Id == vm.EstadoId, ct);
        var existePrioridad = await _context.Prioridades.AnyAsync(p => p.Id == vm.PrioridadId, ct);
        var existeArea = await _context.Unidades.AnyAsync(a => a.Id == vm.UnidadId, ct);
        var existeCategoria = vm.CategoriaId == null || await _context.Categorias.AnyAsync(c => c.Id == vm.CategoriaId, ct);

        if (!ModelState.IsValid)
        {
            await CargarSelects(vm, ct);
            return View(vm);
        }

        //mapeo
        var entidad = new Tarea
        {
            Titulo = vm.Titulo.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(vm.Descripcion) ? null : vm.Descripcion.Trim(),
            FechaCreacion =  DateTime.Now.ToLocalTime(),
            EstadoId = vm.EstadoId,
            PrioridadId = vm.PrioridadId,
            CategoriaId = vm.CategoriaId,
            UnidadId = vm.UnidadId,
            FechaLimite = vm.FechaLimite,
            Archivada = vm.Archivada
        };

        _context.Add(entidad);
        try
        {
            await _context.SaveChangesAsync(ct);
            TempData["Success"] = "La tarea fue creada correctamente.";
            // PRG al detalle para ver el resultado inmediato
            return RedirectToAction(nameof(Details), new { id = entidad.Id });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "No se pudo guardar la tarea. Verifica los datos e intenta nuevamente.");
            await CargarSelects(vm, ct);
            return View(vm);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error inesperado: {ex.GetBaseException().Message}");
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
                    EstadoId = t.EstadoId,
                    PrioridadId = t.PrioridadId,
                    CategoriaId = t.CategoriaId,
                    UnidadId = t.UnidadId,
                    FechaLimite = t.FechaLimite.ToLocalTime()
                })
                .FirstOrDefaultAsync(ct);

        if (vm is null) return NotFound();

        await CargarSelects(vm, ct);
        return View(vm);
    }

    // POST: /Tareas/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TareaFormVM vm, CancellationToken ct)
    {
        if (id != vm.Id) return BadRequest("Identificadores no coinciden.");

        //validar la fechaLimite
        if (vm.FechaLimite < DateTime.Now.Date)
            ModelState.AddModelError(nameof(vm.FechaLimite), "La fecha límite no puede ser anterior a hoy.");

        // Validar relaciones
        var existeEstado = await _context.Estados.AnyAsync(e => e.Id == vm.EstadoId, ct);
        var existePrioridad = await _context.Prioridades.AnyAsync(p => p.Id == vm.PrioridadId, ct);
        var existeArea = await _context.Unidades.AnyAsync(a => a.Id == vm.UnidadId, ct);
        var existeCategoria = vm.CategoriaId == null || await _context.Categorias.AnyAsync(c => c.Id == vm.CategoriaId, ct);

        if (!existeEstado) ModelState.AddModelError(nameof(vm.EstadoId), "Estado no válido.");
        if (!existePrioridad) ModelState.AddModelError(nameof(vm.PrioridadId), "Prioridad no válida.");
        if (!existeArea) ModelState.AddModelError(nameof(vm.UnidadId), "Área no válida.");
        if (!existeCategoria) ModelState.AddModelError(nameof(vm.CategoriaId), "Categoría no válida.");

        if (!ModelState.IsValid)
        {
            await CargarSelects(vm,ct);
            return View(vm);
        }

        // Carga la entidad a editar (TRACKING)
        var t = await _context.Tareas
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (t is null) return NotFound();

        // Concurrencia (si RowVersion existe en BD)
        if (t.RowVersion is not null && vm.RowVersion is not null && !t.RowVersion.SequenceEqual(vm.RowVersion))
        {
            ModelState.AddModelError(string.Empty, "La tarea fue modificada por otro usuario. Refresca la página e intenta de nuevo.");
            await CargarSelects(vm, ct);
            return View(vm);
        }

        // Mapeo explícito: solo campos editables
        t.Id = id;
        t.Titulo = vm.Titulo.Trim();
        t.Descripcion = string.IsNullOrWhiteSpace(vm.Descripcion) ? null : vm.Descripcion.Trim();
        t.EstadoId = vm.EstadoId;
        t.PrioridadId = vm.PrioridadId;
        t.CategoriaId = vm.CategoriaId;
        t.UnidadId = vm.UnidadId;
        t.FechaLimite =  vm.FechaLimite.ToLocalTime();
        t.Archivada = vm.Archivada;
        t.FechaActualizacion = DateTime.Now.ToLocalTime();

        try
        {
            await _context.SaveChangesAsync(ct);
            TempData["Success"] = "La tarea fue actualizada correctamente.";
            return RedirectToAction(nameof(Details), new { id = t.Id });
        }
        catch (DbUpdateConcurrencyException)
        {
            // Si implementas RowVersion (ver abajo), aquí detectas ediciones simultáneas
            ModelState.AddModelError(string.Empty, "Otro usuario modificó este registro. Actualiza la página y vuelve a intentarlo.");
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "No se pudo guardar los cambios. Verifica los datos e intenta de nuevo.");
        }

        await CargarSelects(vm, ct);
        return View(vm);
    }

    // GET: /Tareas/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var t = await _context.Tareas
           .Include(x => x.Estado)
           .Include(x => x.Prioridad)
           .Include(x => x.Unidad)
           .FirstOrDefaultAsync(m => m.Id == id,ct);
        if (t == null) return NotFound();
        return View(t);
    }

    // POST: /Tareas/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var t = await _context.Tareas.FindAsync(id);
        if (t == null) return NotFound();

        try
        {
            _context.Tareas.Remove(t);
            await _context.SaveChangesAsync(ct);
            TempData["Success"] = "La tarea fue eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "La tarea fue modificada o eliminada por otro usuario. Actualiza la página.";
            return RedirectToAction(nameof(Details));
        }
        catch (DbUpdateException)
        {
            // Posible restricción por FK (p. ej., comentarios/adjuntos)
            TempData["Error"] = "No se pudo eliminar la tarea porque existen datos relacionados.";
            return RedirectToAction(nameof(Details));
        }
    }
    [HttpGet]
    public async Task<IActionResult> Assign()
    {
        ViewBag.Estados = await _context.Estados
            .OrderBy(e => e.Id)
            .Select(e => new SelectListItem(e.Nombre, e.Id.ToString()))
            .ToListAsync();
        var disponibles = await _context.Tecnicos.Select(t=>t.Disponible==false).ToListAsync();
        return View(disponibles);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int id, int TecnicoId)
    {
        var ticket = await _context.Tareas.FindAsync(id);
        if (ticket == null) return NotFound();

        ticket.TecnicoId = TecnicoId;
        ticket.FechaAsignacion = DateTime.Now.ToLocalTime();
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }

    private async Task CargarSelects(TareaFormVM vm, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        vm.Estados = await _context.Estados
            .OrderBy(e => e.Id)
            .Select(e => new SelectListItem(e.Nombre, e.Id.ToString()))
            .ToListAsync(ct);

        ct.ThrowIfCancellationRequested();

        vm.Prioridades = await _context.Prioridades
            .OrderBy(p => p.Id)
            .Select(p => new SelectListItem(p.Nombre, p.Id.ToString()))
            .ToListAsync(ct);

        ct.ThrowIfCancellationRequested();

        vm.Categorias = await _context.Categorias
            .OrderBy(c => c.Nombre)
            .Select(c => new SelectListItem(c.Nombre, c.Id.ToString()))
            .ToListAsync(ct);

        ct.ThrowIfCancellationRequested();

        vm.Unidades = await _context.Unidades
            .OrderBy(u => u.Nombre)
            .Select(u => new SelectListItem(u.Nombre, u.Id.ToString()))
            .ToListAsync(ct);
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
}
