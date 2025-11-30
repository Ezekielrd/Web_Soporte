using Microsoft.AspNetCore.SignalR;

namespace DGASoporte.Hubs
{
    public class NotificacionesHub : Hub
    {
        // Se ejecuta cuando un cliente se conecta
        public override async Task OnConnectedAsync()
        {
            // Aquí podrías loguear algo o manejar grupos
            await base.OnConnectedAsync();
        }

        // Método opcional de prueba que podrás invocar desde JS
        public async Task EnviarPrueba(string mensaje)
        {
            await Clients.Caller.SendAsync("ReceiveNotification", new
            {
                Titulo = "Notificación de prueba",
                Mensaje = mensaje,
                Tipo = "info",
                Url = (string?)null
            });
        }
    }
}
