using DGASoporte.Data;
using DGASoporte.Models;
using DGASoporte.Models.Enumeradores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DGASoporte.Controllers
{
    public class DashboardController : Controller
    {
         private readonly DGADbContext _context;

        public DashboardController(DGADbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await ConstruirDashboardAsync();

            return PartialView("_dashboard", vm); // luego creamos esta vista parcial
        }

        private async Task<DashboardVM> ConstruirDashboardAsync()
        {
            var hoy = DateTime.Today;
            var hace30Dias = hoy.AddDays(-30);

            // Para tendencias: últimos 12 meses (mes actual incluido)
            var inicio12Meses = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-11);

            var vm = new DashboardVM();

            // ========= 1. KPIs (cards) =========

            var totalSolicitudes = await _context.Solicitudes.CountAsync();

            var solicitudesCerradas30 = await _context.Solicitudes
                .Where(s => s.Estado == EstadoS.Cerrada &&
                            (s.FechaActualizacion ?? s.FechaCreacion) >= hace30Dias)
                .CountAsync();

            // Tareas abiertas = sin FechaCierre y no archivadas
            var tareasAbiertas = await _context.Tareas
                .Where(t => !t.Archivada && !t.FechaCierre.HasValue)
                .CountAsync();

            // SLA: tareas con FechaLimite y FechaCierre
            var tareasConSla = await _context.Tareas
                .Where(t => t.FechaLimite.HasValue && t.FechaCierre.HasValue)
                .ToListAsync();

            double porcentajeSla = 0;
            if (tareasConSla.Any())
            {
                var cumplidas = tareasConSla
                    .Count(t => t.FechaCierre!.Value <= t.FechaLimite!.Value);

                porcentajeSla = (double)cumplidas / tareasConSla.Count * 100.0;
            }

            vm.Kpis = new DashboardKpiVM
            {
                TotalSolicitudesHistorico = totalSolicitudes,
                SolicitudesCerradas30Dias = solicitudesCerradas30,
                TareasAbiertasActuales = tareasAbiertas,
                PorcentajeSlaCumplido = Math.Round(porcentajeSla, 1)
            };

            // ========= 2. Tendencia solicitudes por mes (últimos 12 meses) =========

            var solicitudes12Meses = await _context.Solicitudes
                .Where(s => s.FechaCreacion >= inicio12Meses)
                .ToListAsync(); // 👉 agrupamos en memoria

            var solicitudesCerradas12Meses = solicitudes12Meses
                .Where(s => s.Estado == EstadoS.Cerrada)
                .ToList();

            vm.SolicitudesPorMes = solicitudes12Meses
                .GroupBy(s => new { s.FechaCreacion.Year, s.FechaCreacion.Month })
                .Select(g => new SerieMesDto
                {
                    Anio = g.Key.Year,
                    Mes = g.Key.Month,
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Anio).ThenBy(x => x.Mes)
                .ToList();

            vm.SolicitudesCerradasPorMes = solicitudesCerradas12Meses
                .GroupBy(s =>
                {
                    var f = s.FechaActualizacion ?? s.FechaCreacion;
                    return new { f.Year, f.Month };
                })
                .Select(g => new SerieMesDto
                {
                    Anio = g.Key.Year,
                    Mes = g.Key.Month,
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Anio).ThenBy(x => x.Mes)
                .ToList();

            // ========= 3. Carga por técnico =========
            // Tareas NO archivadas, en memoria para manejar null de Tecnico sin drama

            var tareasConTecnico = await _context.Tareas
                .Include(t => t.Tecnico).ThenInclude(te => te.Usuario)
                .Where(t => !t.Archivada)
                .ToListAsync();

            vm.CargaPorTecnico = tareasConTecnico
                .GroupBy(t => t.TecnicoId)
                .Select(g =>
                {
                    var ejemplo = g.FirstOrDefault();
                    string nombreTecnico = "Sin técnico";

                    if (ejemplo?.Tecnico != null)
                    {
                        // AJUSTA esta línea al nombre real en tu entidad Tecnico:
                        // ejemplo.Tecnico.Nombre, ejemplo.Tecnico.NombreCompleto, etc.
                        nombreTecnico = ejemplo.Tecnico.Usuario.NombreCompleto ?? $"Técnico {ejemplo.Tecnico.Id}";
                    }

                    var abiertas = g.Count(t => !t.FechaCierre.HasValue);
                    var cerradas30 = g.Count(t =>
                        t.FechaCierre.HasValue &&
                        t.FechaCierre.Value >= hace30Dias);

                    return new CargaTecnicoDto
                    {
                        TecnicoId = g.Key,
                        NombreTecnico = nombreTecnico,
                        TareasAbiertas = abiertas,
                        TareasCerradas30Dias = cerradas30
                    };
                })
                .OrderByDescending(x => x.TareasAbiertas)
                .ToList();

            // ========= 4. Distribución por estado de solicitud =========

            var solicitudesEstados = await _context.Solicitudes
                .GroupBy(s => s.Estado)
                .Select(g => new
                {
                    Estado = g.Key,
                    Cant = g.Count()
                })
                .ToListAsync();

            vm.DistribucionPorEstado = solicitudesEstados
                .Select(x => new EstadoDistribucionDto
                {
                    Estado = x.Estado.HasValue ? x.Estado.Value.ToString() : "Sin estado",
                    Cantidad = x.Cant
                })
                .ToList();

            // ========= 5. Top unidades por solicitudes =========
            // Todo en memoria para evitar problemas con navegaciones nulas

            var solicitudesConUnidad = await _context.Solicitudes
                .Include(s => s.Unidad)
                .ToListAsync();

            vm.TopUnidadesPorSolicitudes = solicitudesConUnidad
                .GroupBy(s => s.UnidadId)
                .Select(g =>
                {
                    var nombre = g
                        .Select(s => s.Unidad?.Nombre)
                        .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                        ?? "Sin unidad";

                    return new UnidadTopDto
                    {
                        UnidadId = g.Key,
                        NombreUnidad = nombre,
                        CantidadSolicitudes = g.Count()
                    };
                })
                .OrderByDescending(x => x.CantidadSolicitudes)
                .Take(5)
                .ToList();

            return vm;
        }


    }
}
