using DGASoporte.Data;
using DGASoporte.Infraestructura;
using DGASoporte.Models;
using DGASoporte.Models.Enumeradores;
using DGASoporte.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Threading;

namespace DGASoporte.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TareaController : Controller
    {
        private readonly DGADbContext _context;
        private readonly AsignacionTareasService _asignacionService;
        private readonly NotificacionService _notificacionService;

        public TareaController(DGADbContext context, AsignacionTareasService asignacionService, NotificacionService notificacionService)
        {
            _context = context;
            _asignacionService = asignacionService;
            _notificacionService = notificacionService;
        }

        // Helper para detectar AJAX
        private bool EsAjax() =>
            Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        // Get Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            dynamic model = new ExpandoObject();

            var hoy = DateTime.Today;
            var hace30dias = hoy.AddDays(-30);

            model.Tareas = await _context.Tareas
                .IgnoreQueryFilters()
                .Include(t => t.Usuario)
                .Include(t => t.Categoria)
                .Include(t => t.Unidad)
                .Include(t => t.TipoServicio)
                .Include(t => t.Tecnico).ThenInclude(te => te.Usuario)
                .Include(t => t.Tecnico).ThenInclude(te => te.Nivel)
                .Where(t => !t.Archivada && t.FechaCreacion >= hace30dias)
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();

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

            return PartialView("_Tareas", model);
        }

        // Get Detalle
        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("Identificador no válido.");

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
                    DivisionId = t.DivisionId ?? 0,
                    DivisionNombre = t.Division != null ? t.Division.Nombre : null,
                    TecnicoId = t.TecnicoId,
                    TecnicoNombre = t.Tecnico != null ? t.Tecnico.Usuario.NombreCompleto : null,
                    Estado = t.Estado,
                    Prioridad = t.Prioridad,
                    EstadoNombre = EnumExtension.GetDisplayName(t.Estado!),
                    PrioridadNombre = EnumExtension.GetDisplayName(t.Prioridad),
                    TipoServicioId = t.TipoServicioId,
                    TipoServicioNombre = t.TipoServicio != null ? t.TipoServicio.Nombre : null,
                    UsuarioId = t.UsuarioId,
                    UsuarioNombre = t.Usuario != null ? t.Usuario.NombreCompleto : null,
                })
                .FirstOrDefaultAsync(ct);

            if (vm is null) return NotFound();

            return PartialView("_TareaDetalle", vm);
        }

        // Get Create
        [HttpGet]
        public async Task<IActionResult> Create(int? solicitudId, CancellationToken ct)
        {
            var vm = new TareaFormVM();
            if (solicitudId.HasValue)
            {
                // Viene desde una solicitud
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Usuario)
                    .FirstOrDefaultAsync(s => s.Id == solicitudId.Value);

                if (solicitud == null || solicitud.Estado != EstadoS.Enviada)
                {
                    Response.StatusCode = 400;
                    return Content("La solicitud no existe o ya fue evaluada.");
                }

                vm.SolicitudId = solicitud.Id;
                vm.UsuarioId = solicitud.UsuarioId;
                vm.Titulo = solicitud.Titulo;
                vm.Descripcion = solicitud.Descripcion;
                vm.UnidadId = solicitud.UnidadId;
                vm.DivisionId = solicitud.DivisionId;
            }
            await CargarSelects(vm, ct);

            return PartialView("_TareaCrear", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TareaFormVM vm, CancellationToken ct)
        {
            // 🔹 Limpiar validaciones de formularios que NO se usan en "Nueva Tarea"
            var keysToClear = ModelState.Keys
                .Where(k => k.StartsWith("DiagnosticoForm.", StringComparison.OrdinalIgnoreCase)
                         || k.StartsWith("PausaForm.", StringComparison.OrdinalIgnoreCase)
                         || k.StartsWith("ComentarioForm.", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToClear)
            {
                ModelState.Remove(key);
            }

            const int ID_CATEGORIA_GENERAL = 5; // Soporte General

            // 🔹 Validar fecha límite
            if (vm.FechaLimite != null && vm.FechaLimite < DateTime.Now.Date)
                ModelState.AddModelError(nameof(vm.FechaLimite), "La fecha límite no puede ser anterior a hoy.");

            // 🔹 Validar existencia de relaciones básicas
            bool existeArea = await _context.Unidades.AnyAsync(a => a.Id == vm.UnidadId, ct);
            if (!existeArea)
                ModelState.AddModelError(nameof(vm.UnidadId), "El área seleccionada no existe.");

            bool existeCategoria = await _context.Categorias.AnyAsync(c => c.Id == vm.CategoriaId, ct);
            if (!existeCategoria)
                ModelState.AddModelError(nameof(vm.CategoriaId), "La categoría seleccionada no existe.");

            // 🔹 Validar TipoServicio SOLO si la categoría NO es general
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
                vm.TipoServicioId = null;
            }
            //Validación División vs Unidad
            if (vm.UnidadId > 0)
            {
                var unidad = await _context.Unidades.FindAsync(vm.UnidadId);
                if (unidad != null && unidad.DivisionId != null && vm.DivisionId == null)
                {
                    ModelState.AddModelError(nameof(vm.DivisionId), "Debe elegir la división correspondiente.");
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarSelects(vm, ct);
                return PartialView("_TareaCrear", vm);
            }

            //usuario vs solicitud
            int usuarioId;
            Solicitud? solicitud = null;

            if (vm.SolicitudId.HasValue)
            {
                solicitud = await _context.Solicitudes
                    .FirstOrDefaultAsync(s => s.Id == vm.SolicitudId.Value, ct);

                if (solicitud == null)
                {
                    ModelState.AddModelError(string.Empty, "La solicitud asociada ya no existe.");
                    await CargarSelects(vm, ct);
                    return PartialView("_TareaCrear", vm);
                }

                //Usuario que hizo la solicitud
                usuarioId = solicitud.UsuarioId;

                solicitud.Estado = EstadoS.Aprobada;
                solicitud.FechaActualizacion = DateTime.Now;             

            }
            else
            {
                //Creada directamente desde Tareas
                usuarioId = vm.UsuarioId.Value;
            }

            //Mapeo entidad
            var entidad = new Tarea
            {
                Titulo = vm.Titulo.Trim(),
                Descripcion = vm.Descripcion.Trim(),
                FechaCreacion = DateTime.Now.ToLocalTime(),
                Prioridad = vm.Prioridad,
                CategoriaId = vm.CategoriaId,
                UnidadId = vm.UnidadId,
                DivisionId = vm.DivisionId,
                FechaLimite = vm.FechaLimite,
                TipoServicioId = (vm.CategoriaId != ID_CATEGORIA_GENERAL)
                    ? vm.TipoServicioId
                    : null,

                UsuarioId = usuarioId,        //usuario solicitante
                SolicitudId = vm.SolicitudId  //tarea/solicitud
            };

            Tecnico? tecnico = null;
            bool asignadaATecnico = false;   


            if (vm.TecnicoId.HasValue)
            {
                tecnico = await _context.Tecnicos
                    .Include(x => x.Usuario)
                    .FirstOrDefaultAsync(x => x.Id == vm.TecnicoId.Value, ct);

                if (tecnico == null)
                {
                    ModelState.AddModelError(nameof(vm.TecnicoId), "El técnico seleccionado no existe.");
                    await CargarSelects(vm, ct);
                    return PartialView("_TareaCrear", vm);
                }

                entidad.TecnicoId = tecnico.Id;
                entidad.FechaAsignacion = DateTime.Now;
                entidad.Estado = EstadoT.Asignado;
                tecnico.Disponible = false;

                asignadaATecnico = true;
            }
            else
            {
                entidad.TecnicoId = null;          // asegúrate de que sea nullable en la entidad
                entidad.Estado = EstadoT.Nuevo;
            }

            _context.Add(entidad);

            if (solicitud != null)
            {
                _context.Solicitudes.Update(solicitud);
            }
            //Si hay técnico, creamos la asignación DESPUÉS de agregar la tarea
            if (asignadaATecnico && tecnico != null)
            {
                _context.Asignaciones.Add(new Asignacion
                {
                    // aquí ya puedes usar la entidad (EF se encarga del Id al guardar)
                    TareaId = entidad.Id,          // mejor usar navegación que TareaId = 0
                    TecnicoId = tecnico.Id,
                    Fecha = DateTime.Now,
                    Modo = "Manual",
                });
            }

            try
            {
                await _context.SaveChangesAsync(ct);
                if (asignadaATecnico && tecnico != null)
                {
                    await _notificacionService.EnviarTareaAsignadaAsync(
                        tecnico.Id,   
                        entidad.Id,          
                        entidad.Titulo
                    );
                }

                if (solicitud != null)
                {
                    await _notificacionService.EnviarResultadoSolicitudAsync(
                        solicitud.UsuarioId,
                        solicitud.Id,
                        solicitud.Titulo,
                        true   // aprobada
                    );
                }
                TempData["Success"] = "La tarea fue creada correctamente.";
                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message;
                ModelState.AddModelError(string.Empty,
                    "No se pudo guardar la tarea. Verifica los datos e intenta nuevamente. Detalle: " + innerExceptionMessage);

                await CargarSelects(vm, ct);
                return PartialView("_TareaCrear", vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.ToString());
                await CargarSelects(vm, ct);
                return PartialView("_TareaCrear", vm);
            }
        }



        //EDIT
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("Identificador no válido.");

            var vm = await _context.Tareas
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TareaFormVM
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Descripcion = t.Descripcion,
                    Prioridad = t.Prioridad,
                    CategoriaId = t.CategoriaId,
                    UnidadId = t.UnidadId,
                    DivisionId = t.DivisionId,
                    FechaLimite = t.FechaLimite.HasValue ? t.FechaLimite.Value.ToLocalTime() : (DateTime?)null,
                    TipoServicioId = t.TipoServicioId,
                    UsuarioId = t.UsuarioId,
                    RowVersion = t.RowVersion
                })
                .FirstOrDefaultAsync(ct);

            if (vm is null) return NotFound();

            await CargarSelects(vm, ct);
            return PartialView("_TareaEditar", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TareaFormVM vm, CancellationToken ct)
        {
            if (id != vm.Id) return BadRequest("Identificadores no coinciden.");

            // Validar fecha límite
            if (vm.FechaLimite != null && vm.FechaLimite < DateTime.Now.Date)
                ModelState.AddModelError(nameof(vm.FechaLimite), "La fecha límite no puede ser anterior a hoy.");

            // Validar relaciones
            var existeArea = await _context.Unidades.AnyAsync(a => a.Id == vm.UnidadId, ct);
            var existeCategoria = await _context.Categorias.AnyAsync(c => c.Id == vm.CategoriaId, ct);
            if (!existeArea) ModelState.AddModelError(nameof(vm.UnidadId), "Área no válida.");
            if (!existeCategoria) ModelState.AddModelError(nameof(vm.CategoriaId), "Categoría no válida.");

            if (!ModelState.IsValid)
            {
                await CargarSelects(vm, ct);
                return PartialView("_TareaEditar", vm);
            }

            var t = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (t is null) return NotFound();

            // Concurrencia
            if (t.RowVersion is not null && vm.RowVersion is not null && !t.RowVersion.SequenceEqual(vm.RowVersion))
            {
                ModelState.AddModelError(string.Empty, "La tarea fue modificada por otro usuario. Refresca la página e intenta de nuevo.");
                await CargarSelects(vm, ct);
                return PartialView("_TareaEditar", vm);
            }

            // Mapeo
            t.Titulo = vm.Titulo.Trim();
            t.Descripcion = vm.Descripcion.Trim();
            t.Prioridad = vm.Prioridad;
            t.CategoriaId = vm.CategoriaId;
            t.DivisionId = vm.DivisionId;
            t.UnidadId = vm.UnidadId;
            t.FechaLimite = vm.FechaLimite.HasValue ? vm.FechaLimite.Value.ToLocalTime() : (DateTime?)null;
            t.Archivada = vm.Archivada;
            t.FechaActualizacion = DateTime.Now.ToLocalTime();
            try
            {
                await _context.SaveChangesAsync(ct);
                return Json(new { success = true });
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "Otro usuario modificó este registro. Actualiza la página y vuelve a intentarlo.");
                await CargarSelects(vm, ct);
                return PartialView("_TareaEditar", vm);
            }
            catch (DbUpdateException ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message;
                ModelState.AddModelError(string.Empty,
                    "No se pudo guardar la tarea. Verifica los datos e intenta nuevamente. Detalle: " + innerExceptionMessage);
                await CargarSelects(vm, ct);
                return PartialView("_TareaEditar", vm);
            }
        }

        // ===================== DELETE =====================
        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("Identificador no válido.");

            var t = await _context.Tareas
                .Include(x => x.Unidad)
                 .Include(t => t.Categoria)
                .Include(t => t.TipoServicio)
                .Include(x => x.Tecnico).ThenInclude(te => te.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id, ct);

            if (t == null) return NotFound();

            return PartialView("_TareaEliminar", t);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            if (id <= 0)
                return Json(new { success = false, message = "El Identificador no es válido." });

            try
            {
                var t = await _context.Tareas.FirstOrDefaultAsync(u => u.Id == id, ct);
                if (t == null)
                    return Json(new { success = false, message = "Tarea no encontrada." });

                _context.Remove(t);
                await _context.SaveChangesAsync(ct);

                // ⬇️ El JS global se encarga de cerrar modal y recargar _tareas
                return Json(new { success = true, message = "Tarea eliminada correctamente" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, message = "La tarea fue modificada o eliminada por otro usuario. Actualiza la página." });
            }
            catch (DbUpdateException)
            {
                return Json(new { success = false, message = "No se pudo eliminar la tarea porque existen datos relacionados" });
            }
        }

        // ===================== ASIGNAR MANUAL =====================
        [HttpGet]
        public async Task<IActionResult> Asignar(int id)
        {
            var tarea = await _context.Tareas
                .Include(t => t.Categoria)
                .Include(t => t.Unidad)
                .Include(t => t.TipoServicio)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarea == null) return NotFound();

            var tecnicos = await _context.Tecnicos
                .Include(t => t.Usuario)
                .Include(t => t.Nivel)
                 .Include(t => t.TareasAsignadas
                         .Where(ta => ta.Estado == EstadoT.Asignado && !ta.Archivada)) // 👈 solo las asignadas

                .ToListAsync();

            var niveles = await _context.Niveles.ToListAsync();

            var vm = new TareaAsignarVM
            {
                Tarea = tarea,
                Tecnicos = tecnicos,
                Niveles = niveles,
                // si la tarea aún no tiene técnico, esto será null y está perfecto
                TecnicoId = tarea.TecnicoId
            };

            return PartialView("_TareaAsignar", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Asignar(int tareaId, int tecnicoId)
        {
            // luego añadimos aquí la validación de tecnicoId <= 0
            if (tareaId <= 0 || tecnicoId <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Debe seleccionar un técnico válido para asignar la tarea."
                });
            }
            var ok = await _asignacionService.AsignarManualAsync(tareaId, tecnicoId);

            return Json(new
            {
                success = ok,
                message = ok
                    ? "Tarea asignada correctamente."
                    : "No se pudo asignar la tarea al técnico seleccionado."
            });
        }


        // ===================== ASIGNAR AUTO =====================
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

        // ===================== HELPERS (SELECTS) =====================
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

            vm.Unidades = await _context.Unidades
                .OrderBy(u => u.Nombre)
                .AsNoTracking()
                .ToListAsync(ct);

            ct.ThrowIfCancellationRequested();

            var tecnicos = await _context.Tecnicos
                .Include(t => t.Usuario)
                .OrderBy(t => t.Usuario.NombreCompleto)
                .Select(t => new
                {
                    t.Id,
                    Nombre = t.Usuario.NombreCompleto,
                    TareasAsignadas = _context.Tareas
                        .Count(ta => ta.TecnicoId == t.Id
                                     && !ta.Archivada
                                     && ta.Estado == EstadoT.Asignado)
                })
                .ToListAsync(ct);

            vm.Tecnicos = tecnicos
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"{t.Nombre} ({t.TareasAsignadas} tareas asig)"
                })
                .ToList();

            ct.ThrowIfCancellationRequested();

            vm.Usuarios = _context.Usuarios
                .Where(u => u.RolId == 3)
                .OrderBy(u => u.NombreCompleto)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.NombreCompleto
                })
                .ToList();
        }

        // ===================== ARCHIVAR / DESARCHIVAR =====================
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
        [HttpGet]
        public async Task<IActionResult> Buscar(string term, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(Array.Empty<object>());

            term = term.Trim();

            var usuarios = await _context.Usuarios
                .Where(u => u.NombreCompleto.Contains(term) && u.RolId==3) 
                .OrderBy(u => u.NombreCompleto)
                .Take(20)                             
                .Select(u => new
                {
                    id = u.Id,
                    nombre = u.NombreCompleto
                })
                .ToListAsync(ct);

            return Json(usuarios);
        }

    }
}