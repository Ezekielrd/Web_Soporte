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
        private readonly NotificacionService _notificacionService;

        public TecnicoController(DGADbContext context, AsignacionTareasService asignacionService, NotificacionService notificacionService)
        {
            _context = context;
            _asignacionService = asignacionService;
            _notificacionService = notificacionService;
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
                .Include(t => t.Diagnosticos)      
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
                TieneDiagnostico = tarea.Diagnosticos.Any(),
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
                },
                 TieneReporte = tarea.TieneReporte
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarTarea(int id, CancellationToken ct)
        {
            var tarea = await _context.Tareas
                .Include(t => t.Tecnico)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tarea == null)
                return NotFound();

            // Usuario actual
            int usuarioId = User.GetRequiredUserId();
            if (usuarioId <= 0)
            {
                TempData["Alert"] = "No se pudo identificar al usuario.";
                return RedirectToAction(nameof(Index));
            }

            // Técnico actual
            var tecnico = await _context.Tecnicos
                .FirstOrDefaultAsync(te => te.Id == usuarioId, ct);

            if (tecnico == null)
            {
                TempData["Alert"] = "No se encontró el técnico asociado al usuario actual.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            // Validar que es el técnico asignado
            if (tarea.TecnicoId.HasValue && tarea.TecnicoId != tecnico.Id)
            {
                TempData["Alert"] = "No estás asignado a esta tarea, no puedes iniciarla.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            // Solo permitir iniciar si está Asignada
            if (tarea.Estado != EstadoT.Asignado)
            {
                TempData["Alert"] = "Solo se puede iniciar una tarea que está en estado 'Asignado'.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            var ahora = DateTime.Now;

            // Iniciar contador si no se ha iniciado aún
            if (!tarea.InicioContador.HasValue)
            {
                tarea.InicioContador = ahora;
            }
            var estadoAnterior = tarea.Estado;


            tarea.Estado = EstadoT.EnProceso;
            tarea.FechaActualizacion = ahora;

            await _context.SaveChangesAsync(ct);

            await _notificacionService.EnviarCambioEstadoTareaAdminAsync(
                 tarea.Id,
                 tarea.Titulo,
                 estadoAnterior?.GetDisplayName() ?? estadoAnterior?.ToString() ?? "N/A",
                 tarea.Estado?.GetDisplayName() ?? tarea.Estado?.ToString() ?? "N/A"
             );

            TempData["Success"] = "La tarea ha sido iniciada y está ahora 'En proceso'.";

            return RedirectToAction(nameof(Detalle), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarDiagnostico([Bind(Prefix = "DiagnosticoForm")] DiagnosticoVM model,CancellationToken ct)
        {
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

            int usuarioId = User.GetRequiredUserId();

            if (usuarioId <= 0)
            {
                TempData["Alert"] = "No se pudo identificar al usuario.";
                return RedirectToAction(nameof(Index));
            }

            var tarea = await _context.Tareas
                .Include(t => t.Tecnico)
                .FirstOrDefaultAsync(t => t.Id == model.TareaId, ct);

            if (tarea == null)
            {
                TempData["Alert"] = $"No se encontró la tarea con Id {model.TareaId}.";
                return RedirectToAction(nameof(Index));
            }

            var tecnico = await _context.Tecnicos
                .FirstOrDefaultAsync(te => te.Id == usuarioId, ct);

            if (tecnico == null)
            {
                TempData["Alert"] = "No se encontró el técnico asociado al usuario actual.";
                return RedirectToAction(nameof(Detalle), new { id = model.TareaId });
            }

            if (tarea.TecnicoId.HasValue && tarea.TecnicoId != tecnico.Id)
            {
                TempData["Alert"] = "No estás asignado a esta tarea, no puedes registrar diagnóstico.";
                return RedirectToAction(nameof(Detalle), new { id = model.TareaId });
            }

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

            // 👇 Aquí ya NO tocamos ni Estado ni InicioContador
            await _context.SaveChangesAsync(ct);

            TempData["Alert"] = "Diagnóstico registrado correctamente.";
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
                .Include(t => t.Diagnosticos)   // 👈 necesario para validar
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tarea == null)
                return NotFound();

            // ✅ Regla: no se puede finalizar ni dejar en espera sin diagnóstico
            if (!tarea.Diagnosticos.Any())
            {
                TempData["Error"] = "Debe registrar un diagnóstico antes de finalizar o dejar la tarea en espera.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            var ahora = DateTime.Now;

            // Detener contador y acumular tiempo
            if (tarea.InicioContador.HasValue)
            {
                var transcurrido = ahora - tarea.InicioContador.Value;
                tarea.TiempoInvertido += transcurrido;
                tarea.InicioContador = null;
            }

            tarea.FechaCierre ??= ahora;
            tarea.FechaActualizacion = ahora;

            await _context.SaveChangesAsync(ct);

            // Interpretamos el resultado para saber si va como resuelta o pendiente
            bool esPendiente = string.Equals(resultado, "EnEspera", StringComparison.OrdinalIgnoreCase);

            // → directo a ReporteSolucion con el flag
            return RedirectToAction(nameof(ReporteSolucion), new
            {
                id = tarea.Id,
                esPendiente
            });
        }


        [HttpGet]
        public async Task<IActionResult> ReporteSolucion(int id, bool? esPendiente, CancellationToken ct)
        {
            var tarea = await _context.Tareas
                .Include(t => t.Usuario)
                .Include(t => t.Tecnico).ThenInclude(te => te.Usuario)
                .Include(t => t.Unidad)
                .Include(t => t.Division)
                .Include(t => t.Categoria)
                .Include(t => t.TipoServicio)
                .Include(t => t.Diagnosticos)              // 👈 importante
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tarea == null)
                return NotFound();

            // si quieres ser estricto:
            if (!tarea.Diagnosticos.Any())
            {
                TempData["Error"] = "Debe registrar un diagnóstico antes de completar el reporte de la incidencia.";
                return RedirectToAction("Detalle", new { id });
            }

            var diag = tarea.Diagnosticos.FirstOrDefault(); // solo hay uno

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
                TipoServicio = tarea.TipoServicio?.Nombre ?? "N/D",

                Estado = tarea.Estado?.ToString() ?? "N/D",
                Prioridad = tarea.Prioridad.ToString(),
                TecnicoAsignado = tarea.Tecnico != null
                    ? (tarea.Tecnico.Usuario?.NombreCompleto
                        ?? tarea.Tecnico.Usuario?.Usher
                        ?? "Técnico")
                    : "Sin asignar",

                TiempoInvertidoTexto = FormatearTiempo(tarea.TiempoInvertido),

                // 🔹 Solución ya guardada (si existía)
                CausaRaiz = tarea.CausaRaiz ?? string.Empty,
                PasosEjecutados = tarea.PasosEjecutados ?? string.Empty,
                AjustesRealizados = tarea.AjustesRealizados,
                ResultadoFinal = tarea.ResultadoFinal ?? string.Empty,
                Recomendaciones = tarea.Recomendaciones,
                MotivoPendiente = tarea.MotivoPendiente ?? string.Empty,

                TieneReporte = tarea.TieneReporte,
                EsPendiente = esPendiente ?? (tarea.Estado == EstadoT.EnEspera),

                // 🔹 Vincular diagnóstico con reporte
                ProblemaDetectado = diag?.Texto    // diagnóstico técnico del problema
            };

            // 👉 Prefill inteligente: solo si el técnico aún no ha escrito nada
            if (diag != null)
            {
                if (string.IsNullOrWhiteSpace(vm.CausaRaiz))
                {
                    // puedes decidir: usar el texto del diagnóstico como base de causa raíz
                    vm.CausaRaiz = diag.Texto;
                }

                if (string.IsNullOrWhiteSpace(vm.PasosEjecutados) &&
                    !string.IsNullOrWhiteSpace(diag.AccionesPropuestas))
                {
                    vm.PasosEjecutados = diag.AccionesPropuestas;
                }

                if (string.IsNullOrWhiteSpace(vm.AjustesRealizados) &&
                    !string.IsNullOrWhiteSpace(diag.ComentariosAdicionales))
                {
                    vm.AjustesRealizados = diag.ComentariosAdicionales;
                }
            }

            return View("ReporteSolucion", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReporteSolucion(ReporteIncidenciaVM vm, CancellationToken ct)
        {
            // 1) Cargar la tarea SIEMPRE (para resumen y validaciones)
            var tarea = await _context.Tareas
                .Include(t => t.Usuario)
                .Include(t => t.Tecnico).ThenInclude(te => te.Usuario)
                .Include(t => t.Unidad)
                .Include(t => t.Division)
                .Include(t => t.Categoria)
                .Include(t => t.TipoServicio)
                .Include(t => t.Diagnosticos)
                .FirstOrDefaultAsync(t => t.Id == vm.Id, ct);

            if (tarea == null)
                return NotFound();

            var diag = tarea.Diagnosticos.FirstOrDefault();

            // 2) Rellenar SIEMPRE los datos de resumen en el VM
            //    (por si hay que volver a mostrar la vista con errores)
            vm.Titulo = tarea.Titulo;
            vm.DescripcionUsuario = tarea.Descripcion ?? string.Empty;
            vm.FechaCreacion = tarea.FechaCreacion;
            vm.FechaCierre = tarea.FechaCierre;

            vm.Solicitante = tarea.Usuario.NombreCompleto;
            vm.Unidad = tarea.Unidad.Nombre;
            vm.Division = tarea.Division?.Nombre ?? "N/D";
            vm.Categoria = tarea.Categoria.Nombre;
            vm.TipoServicio = tarea.TipoServicio?.Nombre ?? "N/D";

            vm.Estado = tarea.Estado?.ToString() ?? "N/D";
            vm.Prioridad = tarea.Prioridad.ToString();
            vm.TecnicoAsignado = tarea.Tecnico != null
                ? (tarea.Tecnico.Usuario?.NombreCompleto
                    ?? tarea.Tecnico.Usuario?.Usher
                    ?? "Técnico")
                : "Sin asignar";

            vm.TiempoInvertidoTexto = $"{(int)tarea.TiempoInvertido.TotalHours:D2} h {tarea.TiempoInvertido.Minutes:D2} m";
            vm.ProblemaDetectado = diag?.Texto;
            vm.TieneReporte = tarea.TieneReporte;

            // 3) Validación: debe existir diagnóstico
            if (!tarea.Diagnosticos.Any())
            {
                TempData["Error"] = "Debe registrar un diagnóstico antes de guardar el reporte de la incidencia.";
                return RedirectToAction("Detalle", new { id = vm.Id });
            }

            // 4) Validación específica si queda pendiente
            if (vm.EsPendiente && string.IsNullOrWhiteSpace(vm.MotivoPendiente))
            {
                ModelState.AddModelError(nameof(vm.MotivoPendiente),
                    "Debe indicar el motivo por el cual la tarea queda en espera.");
            }

            // 5) Si hay errores de validación, volvemos a mostrar la misma vista
            if (!ModelState.IsValid)
            {
                return View("ReporteSolucion", vm);
            }

            // 6) Mapear a entidad
            var esNuevoReporte = !tarea.TieneReporte;

            tarea.CausaRaiz = vm.CausaRaiz;
            tarea.PasosEjecutados = vm.PasosEjecutados;
            tarea.AjustesRealizados = vm.AjustesRealizados;
            tarea.ResultadoFinal = vm.ResultadoFinal;
            tarea.Recomendaciones = vm.Recomendaciones;
            tarea.MotivoPendiente = vm.EsPendiente ? vm.MotivoPendiente : null;

            tarea.Estado = vm.EsPendiente ? EstadoT.EnEspera : EstadoT.Resuelta;

            tarea.FechaCierre ??= DateTime.Now;
            tarea.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync(ct);

            bool resuelta = tarea.Estado == EstadoT.Resuelta;

            await _notificacionService.EnviarTareaFinalizadaAsync(
                tarea.Id,
                tarea.Titulo,
                resuelta
            );

            TempData["Success"] = esNuevoReporte
                ? "Reporte de solución registrado correctamente."
                : "Reporte de solución actualizado correctamente.";

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
