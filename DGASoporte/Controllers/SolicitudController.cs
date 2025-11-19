using DGASoporte.Data;
using DGASoporte.Hubs;
using DGASoporte.Infraestructura;
using DGASoporte.Models;
using DGASoporte.Models.Enumeradores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace DGASoporte.Controllers
{
    public class SolicitudController : Controller
    {
        private readonly DGADbContext _context;
        private readonly ILogger<SolicitudController> _logger;
        private readonly IHubContext<NotificacionesHub> _notificacionesHub;


        public SolicitudController(DGADbContext context, ILogger<SolicitudController> logger, IHubContext<NotificacionesHub> notificacionesHub)
        {
            _context = context;
            _logger = logger;
            _notificacionesHub = notificacionesHub;
        }
        // GET: Solicitudes

        [HttpGet]
        public async Task<IActionResult> Index(string? estado, string? search, int? tipoId)
        {
            int solicitanteId = User.GetRequiredUserId();

            var solicitudes = await _context.Solicitudes
                .Where(s=>s.UsuarioId==solicitanteId)
                .Include(s => s.Usuario)
                .Include(s => s.Unidad)
                .Include(s => s.TipoIncidencia)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoS>(estado, out var estadoEnum))
            {
                solicitudes = solicitudes.Where(s => s.Estado == estadoEnum).ToList();
            }

            if (tipoId.HasValue)
            {
                solicitudes = solicitudes.Where(s => s.TipoIncidenciaId == tipoId.Value).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                solicitudes = solicitudes.Where(s =>
                    s.Titulo.Contains(search) ||
                    s.Descripcion.Contains(search) ||
                    s.Usuario.NombreCompleto.Contains(search) ||
                    s.Unidad.Nombre.Contains(search) ||
                    s.TipoIncidencia.Nombre.Contains(search)
                ).ToList();
            }

            // Calcular estadísticas
            ViewBag.TotalSolicitudes = solicitudes.Count;
            ViewBag.NuevasSolicitudes = solicitudes.Count(s => s.Estado == EstadoS.Nueva);
            ViewBag.EnEsperaSolicitudes = solicitudes.Count(s => s.Estado == EstadoS.EnEspera);
            ViewBag.AprobadaSolicitudes = solicitudes.Count(s => s.Estado == EstadoS.Aprobada);
            ViewBag.RechazadaSolicitudes = solicitudes.Count(s => s.Estado == EstadoS.Rechazada);
            ViewBag.EnTareaSolicitudes = solicitudes.Count(s => s.Estado == EstadoS.ConvertidaEnTarea);

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

            ViewBag.CurrentEstado = estado;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentTipo = tipoId;

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
            //datos que no viene desde el form
            ModelState.Remove(nameof(vm.UsuarioId));
            ModelState.Remove(nameof(vm.Estado));

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
                Estado = EstadoS.Enviada
            };

            _context.Solicitudes.Add(entidad);
            await _context.SaveChangesAsync();

            TempData["ok"] = "Solicitud registrada con éxito.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarUnidadesAsync(SolicitudVM vm, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            vm.Unidades = await _context.Unidades
                .OrderBy(u => u.Nombre)
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Nombre })
                .ToListAsync();

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
    }
}