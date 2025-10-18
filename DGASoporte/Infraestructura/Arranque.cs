using DGASoporte.Data;
using DGASoporte.Models;
using DGASoporte.Seguridad;
using Microsoft.EntityFrameworkCore;

namespace DGASoporte.Infraestructura
{
    public class Arranque
    {
        public static async Task EnsureSeedAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DGADbContext>();

            await ctx.Database.MigrateAsync();

            // 1) Roles
            string[] roles = new[] { "Admin", "Soporte", "Cliente" };
            foreach (var r in roles)
                if (!await ctx.Roles.AnyAsync(x => x.Nombre == r))
                    ctx.Roles.Add(new Rol { Nombre = r });
            await ctx.SaveChangesAsync();

            // 2) Admin por defecto (solo si no hay ninguno con rol Admin)
            var adminRoleId = await ctx.Roles.Where(r => r.Nombre == "Admin")
                                             .Select(r => r.Id)
                                             .FirstAsync();

            bool anyAdmin = await ctx.Usuarios.AnyAsync(u => u.RolId == adminRoleId);
            if (!anyAdmin)
            {
                var email = "admin@demo.com";
                var (hash, salt) = PasswordHasher.Hash("Admin123!");

                var admin = new Usuario
                {
                    // Requeridos / NOT NULL en tu BD:
                    User = email,
                    Email = email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Activo = true,
                    Bloqueado = false,       
                    AccesoFallado = 0,      
                    RolId = adminRoleId,
                    CreadoEn = DateTime.UtcNow,
                    NombreCompleto = "Administrador del sistema",
                    Codigo = "ADM-0001"    
                };

                ctx.Usuarios.Add(admin);
                await ctx.SaveChangesAsync();
            }
        }
    }
}
