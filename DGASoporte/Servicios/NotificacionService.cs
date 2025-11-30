using Azure.Core;
using DGASoporte.Data;
using DGASoporte.Hubs;
using DGASoporte.Migrations;
using DGASoporte.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;

namespace DGASoporte.Servicios
{
    public class NotificacionService
    {
        private readonly DGADbContext _ctx;
        private readonly IHubContext<NotificacionesHub> _hub;

        public NotificacionService(
            DGADbContext ctx,
            IHubContext<NotificacionesHub> hub)
        {
            _ctx = ctx;
            _hub = hub;
        }
        public async Task EnviarResultadoSolicitudAsync(
    int usuarioDestinoId,
    int solicitudId,
    bool aprobada)
        {
            string titulo;
            string mensaje;
            string icono;

            if (aprobada)
            {
                titulo = "Solicitud aprobada";
                mensaje = $"Tu solicitud #{solicitudId} ha sido aprobada.";
                icono = "success";
            }
            else
            {
                titulo = "Solicitud rechazada";
                mensaje = $"Tu solicitud #{solicitudId} ha sido rechazada.";
                icono = "warning";
            }

            // El solicitante verá su solicitud en el Index con el modal de detalle
            string urlDestino = $"/Solicitudes?solicitudId={solicitudId}";

            var notificacion = new Notificacion
            {
                UsuarioId = usuarioDestinoId,
                Tipo = "ResultadoSolicitud",
                Titulo = titulo,
                Mensaje = mensaje,
                UrlDestino = urlDestino,
                FechaCreacion = DateTime.Now,
                Leida = false
            };

            _ctx.Notificaciones.Add(notificacion);
            await _ctx.SaveChangesAsync();

            await _hub.Clients
                .User(usuarioDestinoId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    Titulo = titulo,
                    Mensaje = mensaje,
                    Tipo = "ResultadoSolicitud", // categoría
                    Icono = icono,                // para Swal
                    Url = urlDestino
                });
        }

        public async Task EnviarTareaAsignadaAsync(
            int usuarioDestinoId,
            int tareaId,
            string tituloTarea)
        {
            string titulo = "Nueva tarea asignada";
            string mensaje = $"Se te ha asignado la tarea #{tareaId}: {tituloTarea}";

            // Vista COMPLETA de detalle de tarea del técnico
            // Ajusta la ruta si tu acción/controlador usan otro nombre
            string urlDestino = $"/Tareas/Detalles/{tareaId}";

            var notificacion = new Notificacion
            {
                UsuarioId = usuarioDestinoId,
                Tipo = "TareaAsignada",
                Titulo = titulo,
                Mensaje = mensaje,
                UrlDestino = urlDestino,
                FechaCreacion = DateTime.Now,
                Leida = false
            };

            _ctx.Notificaciones.Add(notificacion);
            await _ctx.SaveChangesAsync();

            await _hub.Clients
                .User(usuarioDestinoId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    Titulo = titulo,
                    Mensaje = mensaje,
                    Tipo = "TareaAsignada", // categoría
                    Icono = "info",           // para Swal
                    Url = urlDestino
                });
        }

        public async Task<List<Notificacion>> ObtenerNotificacionesUsuarioAsync(
     int usuarioId,
     bool soloNoLeidas = false,
     int max = 20,
     bool excluirNuevaSolicitudPropia = true)
        {
            var query = _ctx.Notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaCreacion)
                .AsQueryable();

            if (soloNoLeidas)
            {
                query = query.Where(n => !n.Leida);
            }

            // 👇 Aquí filtramos las notificaciones de tipo "NuevaSolicitud"
            // (el tipo exacto ajústalo al que usas: "NuevaSolicitud", "NuevaSolicitudSoporte", etc.)
            if (excluirNuevaSolicitudPropia)
            {
                query = query.Where(n => n.Tipo != "NuevaSolicitud");
            }

            var data = await query
                .Take(max)
                .Select(n => new Notificacion
                {
                    Id = n.Id,
                    Tipo = n.Tipo,
                    Titulo = n.Titulo,
                    Mensaje = n.Mensaje,
                    UrlDestino = n.UrlDestino,
                    FechaCreacion = n.FechaCreacion,
                    Leida = n.Leida,
                    UsuarioId = n.UsuarioId,
                })
                .ToListAsync();

            return data;
        }


        // 🔹 Marcar todas como leídas para un usuario
        public async Task MarcarTodasLeidasAsync(int usuarioId)
        {
            var notifs = await _ctx.Notificaciones
                .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                .ToListAsync();

            if (!notifs.Any())
                return;

            foreach (var n in notifs)
            {
                n.Leida = true;
            }

            await _ctx.SaveChangesAsync();
        }

        public async Task MarcarLeidaAsync(int id, int usuarioId)
        {
            var notif = await _ctx.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == usuarioId);

            if (notif == null)
                return;

            notif.Leida = true;
            await _ctx.SaveChangesAsync();
        }
    }

    
}
