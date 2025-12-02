using DGASoporte.Data;
using DGASoporte.Infraestructura;
using DGASoporte.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DGASoporte.Controllers
{
    [Authorize]
    public class NotificacionesController : Controller
    {
        private readonly NotificacionService _notificacionService;
        private readonly DGADbContext _context;

        public NotificacionesController(NotificacionService notificacionService, DGADbContext context)
        {
            _notificacionService = notificacionService;
            _context = context;
        }

        // GET: /Notificaciones/Lista?soloNoLeidas=true
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            int usuarioId = User.GetRequiredUserId();
            if (usuarioId<=0)
                return Unauthorized();


            var notifs = await _notificacionService
                .ObtenerNotificacionesUsuarioAsync(usuarioId, soloNoLeidas: true);


            return Json(notifs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarTodasComoLeidas()
        {
            int userId = User.GetRequiredUserId();

            var notisNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leida)
                .ToListAsync();

            foreach (var n in notisNoLeidas)
            {
                n.Leida = true;
            }

            await _context.SaveChangesAsync();

            // 👇 en vez de RedirectToAction(nameof(Index))
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            int usuarioId = int.Parse(userIdStr);

            await _notificacionService.MarcarLeidaAsync(id, usuarioId);

            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> Resumen()
        {
            int userId = User.GetRequiredUserId();

            int unreadCount = await _context.Notificaciones
                .CountAsync(n => n.UsuarioId == userId && !n.Leida);

            var ultimas = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId
                         && !n.Leida
                         && n.Tipo != "SolicitudSolicitud") // 👈 excluyes las de “nueva solicitud”
                .OrderByDescending(n => n.FechaCreacion)
                .Take(5)
                .Select(n => new
                {
                    id = n.Id,
                    titulo = n.Titulo,
                    mensaje = n.Mensaje,
                    leida = n.Leida,
                    fecha = n.FechaCreacion.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(new
            {
                unreadCount,
                items = ultimas
            });
        }
        [HttpGet]
        public async Task<IActionResult> Abrir(int id)
        {
            int userId = User.GetRequiredUserId();

            var noti = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == userId);

            if (noti == null)
                return NotFound();

            if (!noti.Leida)
            {
                noti.Leida = true;
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(noti.UrlDestino))
                return Redirect(noti.UrlDestino);   // 👈 te manda al modal/detalle que ya tienes

            return Redirect("~/");
        }
    }
}
