using DGASoporte.Data;
using DGASoporte.Infraestructura;
using DGASoporte.Models;
using DGASoporte.Models.Enumeradores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace DGASoporte.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class SolicitudController : Controller
    {
        private readonly DGADbContext _context;

        public SolicitudController(DGADbContext context, ILogger<SolicitudController> logger)
        {
            _context = context;
        }
        // GET: Solicitudes

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int solicitanteId = User.GetRequiredUserId();
            var hoy = DateTime.Today;
            var hace30dias = hoy.AddDays(-30);
            var solicitudes = await _context.Solicitudes
                .Where(s=>s.UsuarioId==solicitanteId)
                .Include(s => s.Usuario)
                .Include(s => s.Unidad)
                .Include(s => s.TipoIncidencia)
                .Where(t => !t.Archivada && t.FechaCreacion >= hace30dias)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            // Calcular estadísticas
            ViewBag.TotalSolicitudes = solicitudes.Count;
            ViewBag.EnEsperaSolicitudes = solicitudes.Count(s => s.Estado == EstadoS.EnEspera);
            ViewBag.AprobadaSolicitudes = solicitudes.Count(s => s.Estado == EstadoS.Aprobada);
            ViewBag.RechazadaSolicitudes = solicitudes.Count(s => s.Estado == EstadoS.Rechazada);
            ViewBag.EnviadaSolicitud = solicitudes.Count(s => s.Estado == EstadoS.Enviada);

            // Cargar datos para filtros
            //cargar tipos de incidencias
            ViewBag.TiposIncidencia = await _context.TipoIncidencias
            .OrderBy(e => e.Id)
            .Select(e => new SelectListItem(e.Nombre, e.Id.ToString()))
            .ToListAsync();
            //cargar estados
            //Obtener los valores del Enum y convertirlos a SelectListItem
            var estadosLista = Enum.GetValues(typeof(EstadoS))
                .Cast<EstadoS>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                })
                .ToList();

            ViewBag.Estados = estadosLista;

            return View(solicitudes);
        }

        // GET: Solicitud/Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new SolicitudVM();
            await CargarUnidadesAsync(vm,ct);
            
            return View(vm);
        }

        // POST: SolicitudIncidencia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SolicitudVM vm, CancellationToken ct)
        {
            bool existeArea = await _context.Unidades.AnyAsync(a => a.Id == vm.UnidadId, ct);
            if (!existeArea)
                ModelState.AddModelError(nameof(vm.UnidadId), "El área seleccionada no existe.");

            bool existeTipoIncidencia = await _context.TipoIncidencias.AnyAsync(c => c.Id == vm.TipoIncidenciaId, ct);
            if (!existeTipoIncidencia)
                ModelState.AddModelError(nameof(vm.TipoIncidenciaId), "La incidecia seleccionada no existe.");

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
                await CargarUnidadesAsync(vm,ct);
                return View(vm);
            }

            // Mapear
            var entidad = new Solicitud
            {
                Titulo = vm.Titulo.Trim(),
                Descripcion = vm.Descripcion.Trim(),
                FechaCreacion = DateTime.Now,
                UnidadId = vm.UnidadId,
                TipoIncidenciaId = vm.TipoIncidenciaId,
                UsuarioId = User.GetRequiredUserId(),
                Estado = EstadoS.Enviada,
                DivisionId = vm.DivisionId
            };

            _context.Solicitudes.Add(entidad);

            await _context.SaveChangesAsync();
            // Enviar a todos los admins conectados
           

            TempData["ok"] = "Solicitud registrada con éxito.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarUnidadesAsync(SolicitudVM vm, CancellationToken ct)
        {
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

            var tipos = await _context.TipoIncidencias
              .Select(t => new { t.Id, t.Nombre, t.Descripcion })
              .ToListAsync();

            ct.ThrowIfCancellationRequested();

            vm.TipoIncidencias = tipos
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Nombre })
                .ToList();

            ct.ThrowIfCancellationRequested();

            vm.MapTipoDesc = tipos.ToDictionary(
                t => t.Id,
                t => t.Descripcion ?? string.Empty
            );
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archivar(int id)
        {
            var solicitud = await _context.Solicitudes.FindAsync(id);
            if (solicitud == null)
                return NotFound();

            // Marcar como archivada
            solicitud.Archivada = true;

            await _context.SaveChangesAsync();

            // Vuelves a la lista
            return RedirectToAction(nameof(Index));
        }

    }
}