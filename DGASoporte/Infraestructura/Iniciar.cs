using DGASoporte.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace DGASoporte.Infraestructura
{
    public class Iniciar
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            // Aplica migraciones pendientes (si usas Migrations)
            var ctx = sp.GetRequiredService<DGADbContext>();
            await ctx.Database.MigrateAsync();

            // Ejecuta tu seeder sin Identity
            await Arranque.EnsureSeedAsync(sp);
        }
    }
}
