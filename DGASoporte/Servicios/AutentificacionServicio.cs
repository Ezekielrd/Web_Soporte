using DGASoporte.Data;
using DGASoporte.Models;
using DGASoporte.Seguridad;
using Microsoft.EntityFrameworkCore;
using System;

namespace DGASoporte.Servicios
{
    public class AutentificacionServicio:IAutentificacionServicio
    {
        private readonly DGADbContext _ctx;

        public AutentificacionServicio(DGADbContext ctx) => _ctx = ctx;

        public Task<Usuario?> FindByEmailAsync(string email) =>
            _ctx.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == email);

        public Task<bool> VerifyPasswordAsync(Usuario user, string password) =>
            Task.FromResult(PasswordHasher.Verificar(password, user.PasswordSalt, user.PasswordHash));

        public async Task<Usuario> CreateUserAsync(string email, string userName, string nombreCompleto, string password, string rolNombre)
        {
            // Rol existente
            var rol = await _ctx.Roles.FirstOrDefaultAsync(r => r.Nombre == rolNombre)
                      ?? throw new InvalidOperationException($"Rol '{rolNombre}' no existe.");

            // Hash + sal
            var (hash, salt) = PasswordHasher.Hash(password);

            var user = new Usuario
            {
                Email = email,
                User = userName,
                PasswordHash = hash,
                PasswordSalt = salt,
                RolId = rol.Id,
                Activo = true,
                CreadoEn = DateTime.UtcNow
            };

            _ctx.Usuarios.Add(user);
            await _ctx.SaveChangesAsync();
            return user;
        }
    }
}
