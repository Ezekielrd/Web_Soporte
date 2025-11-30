using DGASoporte.Data;
using DGASoporte.Infraestructura;
using DGASoporte.Models;
using DGASoporte.Models.Enumeradores;
using DGASoporte.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using System.Security.Claims;

namespace DGASoporte.Controllers
{
    [Authorize(Roles = "Tecnico")]  
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

            var hoy = DateTime.Today;
            var hace30dias = hoy.AddDays(-30);

            var ordenEstados = new Dictionary<EstadoT, int>
            {
                { EstadoT.EnEspera,  1 },
                { EstadoT.EnProceso,  2 },
                { EstadoT.Asignado, 3 },
                { EstadoT.Pausado,   4 }, 
                { EstadoT.Resuelta,  5 }
            };

            var listaTareas = await _context.Tareas
            .Include(t => t.Categoria)
            .Include(t => t.Unidad)
            .Include(t => t.Tecnico)
                .ThenInclude(te => te.Usuario)
            .Where(t =>
                !t.Archivada &&
                t.Tecnico != null &&
                t.Tecnico.Id == tecnicoId &&
                t.FechaCreacion >= hace30dias)
            .ToListAsync();

            var tareas = listaTareas
             .OrderBy(t =>
             {
                 var estado = t.Estado ?? EstadoT.EnEspera;
                 if (ordenEstados.TryGetValue(estado, out var orden))
                 {
                     return orden;        
                 }
                 return int.MaxValue;
             })
             .ToList();


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
                .Include(t => t.Historial)
                    .ThenInclude(c => c.Usuario)
                .Include(t => t.Categoria)
                .Include(t => t.Usuario)
                .Include(t => t.Unidad)
                .Include(t => t.Tecnico)!.ThenInclude(te => te.Usuario)
                .Include(t => t.TipoServicio)
                .Include(t => t.Diagnosticos)      // 👈 importante
                    .ThenInclude(d => d.Tecnico)
                        .ThenInclude(te => te.Usuario)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tarea == null)
            {
                TempData["Alert"] = $"No se encontró la tarea con Id {id}.";
                return RedirectToAction(nameof(Index));
            }

            // Si el contador está activo y se usa, puedes mantener esto;
            // si ya no lo usas, puedes eliminar este bloque.
            if (tarea.Estado == EstadoT.EnProceso && tarea.InicioContador.HasValue)
            {
                var transcurrido = DateTime.Now - tarea.InicioContador.Value;
                tarea.TiempoInvertido += transcurrido;
                tarea.InicioContador = DateTime.Now;
                await _context.SaveChangesAsync(ct);
            }

            // Último diagnóstico
            var ultimoDiag = tarea.Diagnosticos
                .OrderByDescending(d => d.FechaRegistro)
                .FirstOrDefault();

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
                DivisionId = tarea.DivisionId,
                DivisionNombre = tarea.Division != null ? tarea.Division.Nombre : null,
                TecnicoId = tarea.TecnicoId,
                TecnicoNombre = tarea.Tecnico?.Usuario.NombreCompleto,
                Estado = tarea.Estado,
                Prioridad = tarea.Prioridad,
                EstadoNombre = EnumExtension.GetDisplayName(tarea.Estado!),
                PrioridadNombre = EnumExtension.GetDisplayName(tarea.Prioridad),
                TipoServicioId = tarea.TipoServicioId,
                TipoServicioNombre = tarea.TipoServicio?.Nombre,
                TiempoInvertido = tarea.TiempoInvertido,
                Historial = tarea.Historial
                    .OrderByDescending(h => h.FechaHora)
                    .ToList(),
                UsuarioId = tarea.UsuarioId,
                UsuarioNombre = tarea.Usuario.NombreCompleto,

                // Datos del último diagnóstico
                DiagnosticoTexto = ultimoDiag?.Texto,
                DiagnosticoAcciones = ultimoDiag?.AccionesPropuestas,
                DiagnosticoComentarios = ultimoDiag?.ComentariosAdicionales,
                DiagnosticoFecha = ultimoDiag?.FechaRegistro,
                DiagnosticoTecnicoNombre = ultimoDiag?.Tecnico?.Usuario.NombreCompleto,
                CodigoDiagnostico = ultimoDiag?.Codigo,

                //Estado del contador + formulario de pausa
                ContadorActivo = tarea.InicioContador.HasValue,
                PausaForm = new PausaVM
                {
                    TareaId = tarea.Id
                },
                // Inicializamos el form del modal
                DiagnosticoForm = new DiagnosticoVM
                {
                    TareaId = tarea.Id
                    // Codigo se escribe en el modal, así que lo dejamos vacío
                },
                ComentarioForm = new ComentarioVM
                {
                    TareaId = tarea.Id
                }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarDiagnostico([Bind(Prefix = "DiagnosticoForm")] DiagnosticoVM model,CancellationToken ct)
        {
            // 1. Validación del formulario
            if (!ModelState.IsValid)
            {
                TempData["Alert"] = "Revise los datos del diagnóstico.";
                return RedirectToAction(nameof(Detalle), new { id = model.TareaId });
            }

            if (model.TareaId <= 0)
            {
                TempData["Alert"] = "La tarea especificada no es válida.";
                return RedirectToAction(nameof(Index));
            }

            // 2. Usuario actual
            int usuarioId = User.GetRequiredUserId();

            if (usuarioId <=0)
            {
                TempData["Alert"] = "No se pudo identificar al usuario.";
                return RedirectToAction(nameof(Index));
            }

            // 3. Tarea
            var tarea = await _context.Tareas
                .Include(t => t.Tecnico)
                .FirstOrDefaultAsync(t => t.Id == model.TareaId, ct);

            if (tarea == null)
            {
                TempData["Alert"] = $"No se encontró la tarea con Id {model.TareaId}.";
                return RedirectToAction(nameof(Index));
            }

            // 4. Técnico actual (a partir del usuario logueado)
            var tecnico = await _context.Tecnicos
                .FirstOrDefaultAsync(te => te.Id == usuarioId, ct);

            if (tecnico == null)
            {
                TempData["Alert"] = "No se encontró el técnico asociado al usuario actual.";
                return RedirectToAction(nameof(Detalle), new { id = model.TareaId });
            }

            // (Opcional) validar que es el técnico asignado
            if (tarea.TecnicoId.HasValue && tarea.TecnicoId != tecnico.Id)
            {
                TempData["Alert"] = "No estás asignado a esta tarea, no puedes registrar diagnóstico.";
                return RedirectToAction(nameof(Detalle), new { id = model.TareaId });
            }

            // 5. Crear diagnóstico
            var diag = new Diagnostico
            {
                TareaId = tarea.Id,
                TecnicoId = tecnico.Id,
                Codigo = string.IsNullOrWhiteSpace(model.Codigo) ? null : model.Codigo.Trim(),
                Texto = model.Texto.Trim(),
                AccionesPropuestas = string.IsNullOrWhiteSpace(model.AccionesPropuestas) ? null : model.AccionesPropuestas.Trim(),
                ComentariosAdicionales = string.IsNullOrWhiteSpace(model.ComentariosAdicionales) ? null : model.ComentariosAdicionales.Trim(),
                FechaRegistro = DateTime.Now
            };

            _context.Diagnosticos.Add(diag);

            // 6. Actualizar la tarea a "En Proceso"
            if (tarea.Estado == EstadoT.Resuelta||tarea.Estado==EstadoT.Asignado)
            {
                tarea.Estado = EstadoT.EnProceso;
            }

            // 7. Iniciar contador si aún no se ha iniciado
            if (!tarea.InicioContador.HasValue)
            {
                tarea.InicioContador = DateTime.Now;
            }

            // 8. Guardar cambios
            await _context.SaveChangesAsync(ct);

            TempData["Alert"] = "Diagnóstico registrado y tarea actualizada a 'En proceso'.";
            return RedirectToAction(nameof(Detalle), new { id = model.TareaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PausarTarea( [Bind(Prefix = "PausaForm")] PausaVM model,CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                TempData["Alert"] = "El motivo de la pausa es obligatorio y debe ser suficientemente descriptivo.";
                return RedirectToAction(nameof(Detalle), new { id = model.TareaId });
            }

            if (model.TareaId <= 0)
            {
                TempData["Alert"] = "La tarea especificada no es válida.";
                return RedirectToAction(nameof(Index));
            }

            int usuarioId = User.GetRequiredUserId();
            if (usuarioId <= 0)
            {
                TempData["Alert"] = "No se pudo identificar al usuario.";
                return RedirectToAction(nameof(Index));
            }

            var tarea = await _context.Tareas
                .FirstOrDefaultAsync(t => t.Id == model.TareaId, ct);

            if (tarea == null)
            {
                TempData["Alert"] = $"No se encontró la tarea con Id {model.TareaId}.";
                return RedirectToAction(nameof(Index));
            }

            // Validar que el contador esté activo
            if (!tarea.InicioContador.HasValue)
            {
                TempData["Alert"] = "La tarea no tiene un contador activo para pausar.";
                return RedirectToAction(nameof(Detalle), new { id = model.TareaId });
            }

            // Sumar el tiempo transcurrido
            var ahora = DateTime.Now;
            var transcurrido = ahora - tarea.InicioContador.Value;
            tarea.TiempoInvertido += transcurrido;

            // Detener contador
            tarea.InicioContador = null;

            // Guardar comentario con el motivo de la pausa
            var comentario = new Comentario
            {
                TareaId = tarea.Id,
                UsuarioId = usuarioId,
                FechaHora = DateTime.Now,
                Texto = $"Pausa registrada: {model.Motivo.Trim()}"
            };

            _context.Comentarios.Add(comentario);

            tarea.Estado = EstadoT.Pausado;
            await _context.SaveChangesAsync(ct);

            TempData["Alert"] = "La tarea ha sido pausada y se registró el motivo en el historial.";
            return RedirectToAction(nameof(Detalle), new { id = model.TareaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReanudarTarea(int TareaId, CancellationToken ct)
        {
            if (TareaId <= 0)
            {
                TempData["Alert"] = "La tarea especificada no es válida.";
                return RedirectToAction(nameof(Index));
            }

            int usuarioId = User.GetRequiredUserId();
            if (usuarioId <= 0)
            {
                TempData["Alert"] = "No se pudo identificar al usuario.";
                return RedirectToAction(nameof(Index));
            }

            var tarea = await _context.Tareas
                .FirstOrDefaultAsync(t => t.Id == TareaId, ct);

            if (tarea == null)
            {
                TempData["Alert"] = $"No se encontró la tarea con Id {TareaId}.";
                return RedirectToAction(nameof(Index));
            }

            // Si ya está corriendo, no tiene sentido reanudar
            if (tarea.InicioContador.HasValue)
            {
                TempData["Alert"] = "La tarea ya tiene el contador activo.";
                return RedirectToAction(nameof(Detalle), new { id = TareaId });
            }

            // No reanudamos si ya está resuelta
            if (tarea.Estado == EstadoT.Resuelta)
            {
                TempData["Alert"] = "La tarea ya está resuelta, no se puede reanudar el tiempo.";
                return RedirectToAction(nameof(Detalle), new { id = TareaId });
            }

            // Reanudar: fijamos nueva marca de inicio
            tarea.InicioContador = DateTime.Now;

            // Aseguramos estado En Proceso
            tarea.Estado = EstadoT.EnProceso;

            // Comentario automático de reanudación
            var comentario = new Comentario
            {
                TareaId = tarea.Id,
                UsuarioId = usuarioId,
                FechaHora = DateTime.Now,
                Texto = "Reanudación del tiempo de atención de la tarea."
            };
            _context.Comentarios.Add(comentario);

            await _context.SaveChangesAsync(ct);

            TempData["Alert"] = "El tiempo de la tarea se ha reanudado correctamente.";
            return RedirectToAction(nameof(Detalle), new { id = TareaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarComentario([Bind(Prefix = "ComentarioForm")] ComentarioVM model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                TempData["Alert"] = "El comentario es obligatorio y debe ser más descriptivo.";
                return RedirectToAction(nameof(Detalle), new { id = model.TareaId });
            }

            if (model.TareaId <= 0)
            {
                TempData["Alert"] = "La tarea especificada no es válida.";
                return RedirectToAction(nameof(Index));
            }

            int usuarioId = User.GetRequiredUserId();
            if (usuarioId <= 0)
            {
                TempData["Alert"] = "No se pudo identificar al usuario.";
                return RedirectToAction(nameof(Index));
            }

            var tarea = await _context.Tareas
                .FirstOrDefaultAsync(t => t.Id == model.TareaId, ct);

            if (tarea == null)
            {
                TempData["Alert"] = $"No se encontró la tarea con Id {model.TareaId}.";
                return RedirectToAction(nameof(Index));
            }

            // Solo permitir comentarios mientras está En Proceso (según lo que pediste)
            if (tarea.Estado != EstadoT.EnProceso)
            {
                TempData["Alert"] = "Solo se pueden registrar comentarios cuando la tarea está En Proceso.";
                return RedirectToAction(nameof(Detalle), new { id = model.TareaId });
            }

            var comentario = new Comentario
            {
                TareaId = tarea.Id,
                UsuarioId = usuarioId,
                FechaHora = DateTime.Now,
                Texto = model.Texto.Trim()
            };

            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync(ct);

            TempData["Alert"] = "Comentario registrado correctamente.";
            return RedirectToAction(nameof(Detalle), new { id = model.TareaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(int id, string resultado, CancellationToken ct)
        {
            var tarea = await _context.Tareas
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tarea == null)
                return NotFound();

            var ahora = DateTime.Now;

            // 1) SIEMPRE: acumular tiempo y detener contador
            if (tarea.InicioContador.HasValue)
            {
                var transcurrido = ahora - tarea.InicioContador.Value;
                tarea.TiempoInvertido += transcurrido;
                tarea.InicioContador = null;
            }

            // 2) Decidir estado según el resultado
            if (string.Equals(resultado, "Resuelta", StringComparison.OrdinalIgnoreCase))
            {
                tarea.Estado = EstadoT.Resuelta;       
                tarea.FechaCierre ??= ahora;              
            }
            else if (string.Equals(resultado, "EnEspera", StringComparison.OrdinalIgnoreCase))
            {
                tarea.Estado = EstadoT.EnEspera;                                                      
            }
            else
            {
                // Por si llega algo raro
                TempData["Error"] = "Resultado de finalización no válido.";
                return RedirectToAction("Detalle", new { id });
            }

            tarea.FechaActualizacion = ahora;
            await _context.SaveChangesAsync(ct);

            // 3) Flujo
            if (tarea.Estado == EstadoT.Resuelta)
            {
                // Ir al formulario de reporte de solución
                return RedirectToAction(nameof(ReporteSolucion), new { id = tarea.Id });
            }

            // Si quedó en espera, volver al detalle
            return RedirectToAction("Detalle", new { id = tarea.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ReporteSolucion(int id, CancellationToken ct)
        {
            var tarea = await _context.Tareas
                .Include(t => t.Usuario)                     // solicitante
                .Include(t => t.Tecnico)
                    .ThenInclude(te => te.Usuario)          // usuario del técnico
                .Include(t => t.Unidad)
                .Include(t => t.Division)
                .Include(t => t.Categoria)
                .Include(t => t.TipoServicio)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tarea == null)
                return NotFound();

            //Solo permitir el reporte cuando la tarea está marcada como RESUELTA
            // Cambia EstadoT.Resuelta por el valor correcto de tu enum si tiene otro nombre
            if (tarea.Estado != EstadoT.Resuelta)
            {
                TempData["Error"] = "Solo se puede registrar el reporte de solución cuando la tarea está marcada como resuelta.";
                return RedirectToAction("Detalle", new { id });
            }

            // Función local para formatear el tiempo invertido
            string FormatearTiempo(TimeSpan tiempo)
                => $"{(int)tiempo.TotalHours:D2} h {tiempo.Minutes:D2} m";

            var vm = new ReporteIncidenciaVM
            {
                Id = tarea.Id,
                Titulo = tarea.Titulo,
                DescripcionUsuario = tarea.Descripcion,
                FechaCreacion = tarea.FechaCreacion,
                FechaCierre = tarea.FechaCierre,

                Solicitante = tarea.Usuario.NombreCompleto,

                Unidad = tarea.Unidad.Nombre,
                Division = tarea.Division?.Nombre ?? "N/D",
                Categoria = tarea.Categoria.Nombre,
                TipoServicio = tarea.TipoServicio?.Nombre?? "N/D",

                Estado = tarea.Estado?.ToString() ?? "N/D",
                Prioridad = tarea.Prioridad.ToString(),

                TecnicoAsignado = tarea.Tecnico != null
                    ? (tarea.Tecnico.Usuario?.NombreCompleto
                        ?? tarea.Tecnico.Usuario?.Usher
                        ?? "Técnico")
                    : "Sin asignar",

                // ⏱️ Tiempo invertido ya acumulado (por el POST Finalizar)
                TiempoInvertidoTexto = FormatearTiempo(tarea.TiempoInvertido),

                // Campos de solución ya guardados (si existían) para que el técnico pueda editarlos
                CausaRaiz = tarea.CausaRaiz,
                PasosEjecutados = tarea.PasosEjecutados,
                AjustesRealizados = tarea.AjustesRealizados,
                ResultadoFinal = tarea.ResultadoFinal,
                Recomendaciones = tarea.Recomendaciones
            };

            return View("ReporteSolucion", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReporteSolucion(ReporteIncidenciaVM vm, CancellationToken ct)
        {
            // 1) Validar el modelo (campos requeridos, longitudes, etc.)
            if (!ModelState.IsValid)
            {
                // Si falla, volvemos a mostrar la misma vista con los errores
                return View("ReporteSolucion", vm);
            }

            // 2) Buscar la tarea en BD
            var tarea = await _context.Tareas
                .Include(t => t.Usuario)
                .Include(t => t.Tecnico).ThenInclude(te => te.Usuario)
                .Include(t => t.Unidad)
                .Include(t => t.Division)
                .Include(t => t.Categoria)
                .Include(t => t.TipoServicio)
                .FirstOrDefaultAsync(t => t.Id == vm.Id, ct);

            if (tarea == null)
                return NotFound();

            // 3) Seguridad extra: solo permitir guardar reporte si está RESUELTA
            if (tarea.Estado != EstadoT.Resuelta) // ajusta el nombre del enum si es otro
            {
                TempData["Error"] = "Solo se puede registrar el reporte de solución cuando la tarea está marcada como resuelta.";
                return RedirectToAction("Detalle", new { id = vm.Id });
            }

            // 4) Mapear los campos del formulario a la entidad Tarea
            tarea.CausaRaiz = vm.CausaRaiz;
            tarea.PasosEjecutados = vm.PasosEjecutados;
            tarea.AjustesRealizados = vm.AjustesRealizados;
            tarea.ResultadoFinal = vm.ResultadoFinal;
            tarea.Recomendaciones = vm.Recomendaciones;

            // FechaCierre ya debió ponerse en Finalizar, pero por si acaso:
            tarea.FechaCierre ??= DateTime.Now;
            tarea.FechaActualizacion = DateTime.Now;

            // (TiempoInvertido ya lo manejaste en el POST Finalizar, aquí no se toca)

            await _context.SaveChangesAsync(ct);

            TempData["Success"] = "Reporte de solución guardado correctamente.";

            // 5) Después de guardar:
            //    puedes mandarlo al detalle de la tarea, o a una vista "reporte listo para imprimir"
            return RedirectToAction("Detalle", new { id = vm.Id });
        }
        [HttpGet]
        public async Task<IActionResult> ReporteIncidencia(int id, CancellationToken ct)
        {
            var tarea = await _context.Tareas
                .Include(t => t.Usuario)
                .Include(t => t.UsuarioValida)
                .Include(t => t.Tecnico).ThenInclude(te => te.Usuario)
                .Include(t => t.Unidad)
                .Include(t => t.Division)
                .Include(t => t.Categoria)
                .Include(t => t.TipoServicio)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tarea == null)
                return NotFound();

            string FormatearTiempo(TimeSpan t) =>
                $"{(int)t.TotalHours:D2} h {t.Minutes:D2} m";

            var vm = new ReporteIncidenciaVM
            {
                Id = tarea.Id,
                Titulo = tarea.Titulo,
                DescripcionUsuario = tarea.Descripcion,
                FechaCreacion = tarea.FechaCreacion,
                FechaCierre = tarea.FechaCierre,

                Solicitante = tarea.Usuario?.NombreCompleto
                              ?? tarea.Usuario?.Usher
                              ?? "N/D",

                Unidad = tarea.Unidad?.Nombre ?? "N/D",
                Division = tarea.Division?.Nombre,
                Categoria = tarea.Categoria?.Nombre ?? "N/D",
                TipoServicio = tarea.TipoServicio?.Nombre,

                Estado = tarea.Estado?.ToString() ?? "N/D",
                Prioridad = tarea.Prioridad.ToString(),

                TecnicoAsignado = tarea.Tecnico != null
                    ? (tarea.Tecnico.Usuario?.NombreCompleto
                        ?? tarea.Tecnico.Usuario?.Usher
                        ?? "Técnico")
                    : "Sin asignar",

                TiempoInvertidoTexto = FormatearTiempo(tarea.TiempoInvertido),

                // Campos de solución llenados por el técnico
                CausaRaiz = tarea.CausaRaiz,
                PasosEjecutados = tarea.PasosEjecutados,
                AjustesRealizados = tarea.AjustesRealizados,
                ResultadoFinal = tarea.ResultadoFinal,
                Recomendaciones = tarea.Recomendaciones
            };

            return View("ReporteIncidencia", vm);
        }
        [HttpGet]
        public async Task<IActionResult> ReporteIncidenciaPdf(int id, CancellationToken ct)
        {
            var tarea = await _context.Tareas
                .Include(t => t.Usuario)
                .Include(t => t.UsuarioValida)
                .Include(t => t.Tecnico).ThenInclude(te => te.Usuario)
                .Include(t => t.Unidad)
                .Include(t => t.Division)
                .Include(t => t.Categoria)
                .Include(t => t.TipoServicio)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tarea == null)
                return NotFound();

            string FormatearTiempo(TimeSpan t) =>
                $"{(int)t.TotalHours:D2} h {t.Minutes:D2} m";

            var vm = new ReporteIncidenciaVM
            {
                Id = tarea.Id,
                Titulo = tarea.Titulo,
                DescripcionUsuario = tarea.Descripcion,
                FechaCreacion = tarea.FechaCreacion,
                FechaCierre = tarea.FechaCierre,
                Solicitante = tarea.Usuario?.NombreCompleto
                              ?? tarea.Usuario?.Usher
                              ?? "N/D",
                Unidad = tarea.Unidad?.Nombre ?? "N/D",
                Division = tarea.Division?.Nombre,
                Categoria = tarea.Categoria?.Nombre ?? "N/D",
                TipoServicio = tarea.TipoServicio?.Nombre,
                Estado = tarea.Estado?.ToString() ?? "N/D",
                Prioridad = tarea.Prioridad.ToString(),
                TecnicoAsignado = tarea.Tecnico != null
                    ? (tarea.Tecnico.Usuario?.NombreCompleto
                        ?? tarea.Tecnico.Usuario?.Usher
                        ?? "Técnico")
                    : "Sin asignar",
                TiempoInvertidoTexto = FormatearTiempo(tarea.TiempoInvertido),
                CausaRaiz = tarea.CausaRaiz,
                PasosEjecutados = tarea.PasosEjecutados,
                AjustesRealizados = tarea.AjustesRealizados,
                ResultadoFinal = tarea.ResultadoFinal,
                Recomendaciones = tarea.Recomendaciones
            };

            return new ViewAsPdf("ReporteIncidencia", vm)
            {
                FileName = $"ReporteIncidencia_{tarea.Id}.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait
            };
        }
    }
}
