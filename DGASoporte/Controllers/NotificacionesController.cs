using DGASoporte.Data;
using DGASoporte.Infraestructura;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DGASoporte.Controllers
{
    public class NotificacionesController : Controller
    {
        private readonly DGADbContext _context;

        public NotificacionesController(DGADbContext context)
        {
            _context = context;
        }
        // Obtener últimas N notificaciones (para el dropdown)
        [HttpGet]
        public async Task<IActionResult> Ultimas(int cantidad = 10)
        {
            var userId = User.GetRequiredUserId();

            var notis = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId)
                .OrderByDescending(n => n.FechaCreacion)
                .Take(cantidad)
                .ToListAsync();

            return Json(notis.Select(n => new {
                n.Id,
                n.Titulo,
                n.Mensaje,
                n.UrlDestino,
                Fecha = n.FechaCreacion.ToString("dd/MM/yyyy HH:mm"),
                n.Leida
            }));
        }

        // Marcar todas como leídas
        [HttpPost]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var userId = User.GetRequiredUserId();

            var notis = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leida)
                .ToListAsync();

            var ahora = DateTime.Now;
            foreach (var n in notis)
            {
                n.Leida = true;
                n.FechaLeida = ahora;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
