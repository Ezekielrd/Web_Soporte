using DGASoporte.Models;

namespace DGASoporte.Servicios
{
    public interface IAutentificacionServicio
    {
        Task<Usuario?> FindByEmailAsync(string email);
        Task<bool> VerifyPasswordAsync(Usuario user, string password);
        Task<Usuario> CreateUserAsync(string email, string userName, string nombreCompleto, string password, string rolNombre);
    }
}
