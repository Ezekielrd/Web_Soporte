using DGASoporte.Data;
using Microsoft.EntityFrameworkCore;

namespace DGASoporte.Infraestructura.Middleware
{
    public class NotificacionesAutoLeidasMiddleware
    {
        private readonly RequestDelegate _next;

        public NotificacionesAutoLeidasMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DGADbContext db)
        {
            // Solo usuarios autenticados
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                // Evitar marcar al entrar a endpoints de notificaciones (para no interferir)
                var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
                if (!path.StartsWith("/notificaciones"))
                {
                    int userId = context.User.GetRequiredUserId();

                    // URL actual (relativa) EXACTA, tal como la guardas en UrlDestino
                    string currentUrl = context.Request.Path + context.Request.QueryString;

                    // Marcar como leídas todas las notificaciones cuyo UrlDestino coincide con la URL actual
                    string pathOnly = context.Request.Path;

                    var pendientes = await db.Notificaciones
                        .Where(n => n.UsuarioId == userId
                                 && !n.Leida
                                 && n.UrlDestino != null
                                 && (n.UrlDestino == currentUrl || n.UrlDestino == pathOnly))
                        .ToListAsync();


                    if (pendientes.Count > 0)
                    {
                        var now = DateTime.Now;
                        foreach (var n in pendientes)
                        {
                            n.Leida = true;
                        }
                        await db.SaveChangesAsync();
                    }
                }
            }

            await _next(context);
        }
    }
}
