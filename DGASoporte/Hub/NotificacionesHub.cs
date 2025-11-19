using Microsoft.AspNetCore.SignalR;

namespace DGASoporte.Hubs
{
    public class NotificacionesHub:Hub
    {
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;

            // grupo de administradores
            if (user?.IsInRole("Administrador") == true || user?.IsInRole("Admin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
            }

            // si luego quieres seguir usando grupos de técnicos:
            var tecnicoIdClaim = user?.FindFirst("TecnicoId")?.Value;
            if (int.TryParse(tecnicoIdClaim, out var tecnicoId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"tecnico-{tecnicoId}");
            }

            await base.OnConnectedAsync();
        }
    }
}
