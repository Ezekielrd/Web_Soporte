using DGASoporte.Data;
using DGASoporte.Models;
using DGASoporte.Models.Enumeradores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace DGASoporte.Controllers
{
    [Authorize(Roles = "Admin,Tecnico")] 
    public class ReportesController : Controller
    {
        private readonly DGADbContext _context;

        public ReportesController(DGADbContext context)
        {
            _context = context;
        }
        private static string FormatearTiempo(TimeSpan t) =>
        $"{(int)t.TotalHours:D2} h {t.Minutes:D2} m";

        /// <summary>
        /// Mapea una Tarea a ReporteIncidenciaVM para HTML y PDF
        /// </summary>
        private ReporteIncidenciaVM MapearReporteIncidenciaVM(Tarea tarea)
        {
            var diag = tarea.Diagnosticos?.FirstOrDefault();

            return new ReporteIncidenciaVM
            {
                Id = tarea.Id,
                Titulo = tarea.Titulo,
                DescripcionUsuario = tarea.Descripcion ?? string.Empty,
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

                // 🔹 Diagnóstico (lo mostramos como problema detectado)
                ProblemaDetectado = diag?.Texto,

                // 🔹 Campos de solución llenados por el técnico
                CausaRaiz = tarea.CausaRaiz ?? string.Empty,
                PasosEjecutados = tarea.PasosEjecutados ?? string.Empty,
                AjustesRealizados = tarea.AjustesRealizados,
                ResultadoFinal = tarea.ResultadoFinal ?? string.Empty,
                Recomendaciones = tarea.Recomendaciones,

                TieneReporte = tarea.TieneReporte,
                EsPendiente = tarea.Estado == EstadoT.EnEspera,
                MotivoPendiente = tarea.MotivoPendiente
            };
        }

        // 🔸 Ver reporte en HTML
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
                .Include(t => t.Diagnosticos)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tarea == null)
                return NotFound();

            var vm = MapearReporteIncidenciaVM(tarea);

            return View("ReporteIncidencia", vm);
        }

        // 🔸 Descargar reporte en PDF
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
                .Include(t => t.Diagnosticos)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tarea == null)
                return NotFound();

            var vm = MapearReporteIncidenciaVM(tarea);

            return new ViewAsPdf("ReporteIncidencia", vm)
            {
                FileName = $"ReporteIncidencia_{tarea.Id}.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait
            };
        }
    }
}
