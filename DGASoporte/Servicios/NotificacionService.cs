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
    string solicitudTitulo,
    bool aprobada)
        {
            string titulo, mensaje, icono;

            if (aprobada)
            {
                titulo = "Solicitud aprobada";
                mensaje = $"Tu solicitud: {solicitudTitulo} ha sido aprobada.";
                icono = "success";
            }
            else
            {
                titulo = "Solicitud rechazada";
                mensaje = $"Tu solicitud: {solicitudTitulo} ha sido rechazada.";
                icono = "warning";
            }

            // 👇 URL donde YA tienes el modal de detalle funcionando
            string urlDestinoReal = $"/Solicitud/Index?solicitudId={solicitudId}";

            var notificacion = new Notificacion
            {
                UsuarioId = usuarioDestinoId,
                Tipo = "ResultadoSolicitud",
                Titulo = titulo,
                Mensaje = mensaje,
                UrlDestino = urlDestinoReal,   // se usará en /Notificaciones/Abrir
                FechaCreacion = DateTime.Now,
                Leida = false
            };

            _ctx.Notificaciones.Add(notificacion);
            await _ctx.SaveChangesAsync();

            // 👇 Link que verá el usuario en la alerta y la campana
            string urlWrapper = $"/Notificaciones/Abrir/{notificacion.Id}";

            await _hub.Clients
                .User(usuarioDestinoId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    Titulo = titulo,
                    Mensaje = mensaje,
                    Tipo = "ResultadoSolicitud",
                    Icono = icono,
                    Url = urlWrapper       // <- OJO: usamos el wrapper, no UrlDestinoReal
                });
        }

        public async Task EnviarTareaAsignadaAsync(
            int usuarioDestinoId,
            int tareaId,
            string tituloTarea)
        {
            string titulo = "Nueva tarea asignada";
            string mensaje = $"Se te ha asignado la tarea: {tituloTarea}";

            // Vista de detalle de tarea del técnico
            string urlDestino = $"/Tecnico/Detalle/{tareaId}";

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

            string urlWrapper = $"/Notificaciones/Abrir/{notificacion.Id}";

            await _hub.Clients
                .User(usuarioDestinoId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    Titulo = titulo,
                    Mensaje = mensaje,
                    Tipo = "TareaAsignada", // categoría
                    Icono = "info",           // para Swal
                    Url = urlWrapper
                });
        }
        public async Task EnviarSolicitudCreadaAdminAsync(int solicitudId, string tituloSolicitud,int usuarioSolicitanteId)
        {
            //obtener admin
            var adminIds = await _ctx.Usuarios
                .Where(u => u.RolId==1)  
                .Select(u => u.Id)
                .ToListAsync();

            if (!adminIds.Any())
                return;

            //Texto de la notificación
            string titulo = "Nueva solicitud de soporte";
            string mensaje = $"Se ha creado la solicitud: {tituloSolicitud}";

            //URL donde se ven las solicitud (lista + modal)
            string urlDestino = $"/Home/Index?view=Solicitudes&solicitudId={solicitudId}";

            foreach (var adminId in adminIds)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = adminId,                
                    Tipo = "SolicitudCreadaAdmin",
                    Titulo = titulo,
                    Mensaje = mensaje,
                    UrlDestino = urlDestino,
                    FechaCreacion = DateTime.Now,
                    Leida = false
                };

                _ctx.Notificaciones.Add(notificacion);
                await _ctx.SaveChangesAsync();

                string urlWrapper = $"/Notificaciones/Abrir/{notificacion.Id}";

                await _hub.Clients
                    .User(adminId.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        Titulo = titulo,
                        Mensaje = mensaje,
                        Tipo = "SolicitudCreadaAdmin",
                        Icono = "info",
                        Url = urlWrapper
                    });
            }
        }
        public async Task EnviarTareaFinalizadaAsync(int tareaId,string tituloTarea,bool resuelta)
        {
            string titulo;
            string mensaje;
            string icono;

            var adminIds = await _ctx.Usuarios
               .Where(u => u.RolId == 1)
               .Select(u => u.Id)
               .ToListAsync();

            if (!adminIds.Any())
                return;

            if (resuelta)
            {
                titulo = "Tarea resuelta";
                mensaje = $"La tarea: {tituloTarea} ha sido marcada como resuelta.";
                icono = "success";
            }
            else
            {
                titulo = "Tarea finalizada con observaciones";
                mensaje = $"La tarea: {tituloTarea} ha sido finalizada, pero requiere revisión adicional.";
                icono = "warning";
            }

            string urlDestinoReal = $"/Home/Index?view=Tareas&tareaId={tareaId}";

            foreach (var adminId in adminIds)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = adminId,
                    Tipo = "TareaFinalizada",
                    Titulo = titulo,
                    Mensaje = mensaje,
                    UrlDestino = urlDestinoReal,
                    FechaCreacion = DateTime.Now,
                    Leida = false
                };

                _ctx.Notificaciones.Add(notificacion);
                await _ctx.SaveChangesAsync();

                string urlWrapper = $"/Notificaciones/Abrir/{notificacion.Id}";

                await _hub.Clients
                    .User(adminId.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        Titulo = titulo,
                        Mensaje = mensaje,
                        Tipo = "TareaFinalizada",
                        Icono = icono,
                        Url = urlWrapper
                    });
            }
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
