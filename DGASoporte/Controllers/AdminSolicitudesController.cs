using DGASoporte.Data;
using DGASoporte.Hubs;
using DGASoporte.Infraestructura;
using DGASoporte.Migrations;
using DGASoporte.Models;
using DGASoporte.Models.Enumeradores;
using DGASoporte.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;


namespace DGASoporte.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSolicitudesController : Controller
    {
        private readonly DGADbContext _context;
        private readonly NotificacionService _notificacionService;

        public AdminSolicitudesController(DGADbContext ctx, NotificacionService notificacionService)
        {
            _context = ctx;
            _notificacionService = notificacionService;
        }

        // Pendientes (Enviadas)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var hace30dias = hoy.AddDays(-30);

            var solicitudes = await _context.Solicitudes
                .Include(s => s.Usuario)
                .Include(s => s.Unidad)
                .Include(s => s.Division)
                .Include(s => s.TipoIncidencia)
                .Where(s => !s.Archivada && s.FechaCreacion >= hace30dias)
                .OrderBy(s =>
                    s.Estado == EstadoS.Enviada ? 1 :
                    s.Estado == EstadoS.Aprobada ? 2 :
                    s.Estado == EstadoS.Rechazada ? 3 :
                    99                           
                )
                .ThenByDescending(s => s.FechaCreacion)
                .ToListAsync();

            return PartialView("_solicitudes", solicitudes);
        }

        [HttpGet]
        public async Task<IActionResult> Rechazar(int id)
        {
            var sol = await _context.Solicitudes.FindAsync(id); 
            if (sol == null) return NotFound();

            ViewBag.SolicitudId = sol.Id;

            return PartialView("_MotivoRechazo");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, string motivoRechazo)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (!isAjax)
            {
                return BadRequest("Esta acción solo admite peticiones AJAX.");
            }

            //Validación del motivo 
            if (string.IsNullOrWhiteSpace(motivoRechazo) || motivoRechazo.Trim().Length < 10)
            {
                return Json(new
                {
                    success = false,
                    message = "Debe indicar un motivo de rechazo con al menos 10 caracteres."
                });
            }

            var solicitud = await _context.Solicitudes
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
            {
                return Json(new
                {
                    success = false,
                    message = "La solicitud no existe."
                });
            }

            if (solicitud.Estado != EstadoS.Enviada)
            {
                return Json(new
                {
                    success = false,
                    message = "La solicitud ya fue evaluada."
                });
            }

            solicitud.Estado = EstadoS.Rechazada;
            solicitud.MotivoRechazo = motivoRechazo;
            solicitud.FechaActualizacion = DateTime.Now;

            _context.Solicitudes.Update(solicitud);
            await _context.SaveChangesAsync();

            await _notificacionService.EnviarResultadoSolicitudAsync(solicitud.UsuarioId, solicitud.Id, solicitud.Titulo, false);


            return Json(new
            {
                success = true,
                message = "Solicitud rechazada correctamente.",
                redirectUrl = Url.Action("Index", "AdminSolicitudes")

            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Usuario)
                .Include(s => s.Unidad)
                .Include(s => s.TipoIncidencia)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null) return NotFound();

            return PartialView("_SolicitudDetalle", solicitud);

        }
    }
}
