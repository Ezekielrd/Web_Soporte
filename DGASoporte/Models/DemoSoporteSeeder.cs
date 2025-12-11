using DGASoporte.Data;
using DGASoporte.Models.Enumeradores;
using Microsoft.EntityFrameworkCore;
using System;

namespace DGASoporte.Models
{
    public class DemoSoporteSeeder
    {
        public static async Task SeedDemoDataAsync(DGADbContext ctx)
        {
            // Si ya tienes suficientes datos, no hacer nada
            if (await ctx.Solicitudes.CountAsync() > 50 || await ctx.Tareas.CountAsync() > 50)
                return;

            var rand = new Random();
            var hoy = DateTime.Today;

            // Listas de FK existentes (si no hay, ponemos 1 para que no falle)
            var usuarios = await ctx.Usuarios.Select(u => u.Id).ToListAsync();
            if (!usuarios.Any()) usuarios.Add(1);

            var tecnicos = await ctx.Tecnicos.Select(t => t.Id).ToListAsync();
            if (!tecnicos.Any()) tecnicos.Add(1);

            var unidades = await ctx.Unidades.Select(u => u.Id).ToListAsync();
            if (!unidades.Any()) unidades.Add(1);

            var divisiones = await ctx.Divisiones.Select(d => d.Id).ToListAsync();
            if (!divisiones.Any()) divisiones.Add(1);

            var tiposInc = await ctx.TipoIncidencias.Select(t => t.Id).ToListAsync();
            if (!tiposInc.Any()) tiposInc.Add(1);

            var categorias = await ctx.Categorias.Select(c => c.Id).ToListAsync();
            if (!categorias.Any()) categorias.Add(1);

            // 1) Generar SOLICITUDES DEMO (últimos 18 meses)
            var solicitudesDemo = new System.Collections.Generic.List<Solicitud>();

            for (int i = 0; i < 200; i++)
            {
                var diasAtras = rand.Next(0, 540); // ~18 meses
                var fechaCreacion = hoy.AddDays(-diasAtras);

                var estado = (EstadoS)rand.Next(1, 5); // Enviada, Aprobada, Rechazada, Cerrada

                var solicitud = new Solicitud
                {
                    Titulo = $"Solicitud demo {i + 1}",
                    Descripcion = "Descripción de prueba para análisis de dashboard.",
                    FechaCreacion = fechaCreacion,
                    UsuarioId = usuarios[rand.Next(usuarios.Count)],
                    UnidadId = unidades[rand.Next(unidades.Count)],
                    DivisionId = divisiones[rand.Next(divisiones.Count)],
                    TipoIncidenciaId = tiposInc[rand.Next(tiposInc.Count)],
                    Estado = estado,
                    MotivoRechazo = estado == EstadoS.Rechazada ? "Motivo demo de rechazo." : null,
                    Archivada = false,
                    FechaActualizacion = fechaCreacion.AddDays(rand.Next(0, 10))
                };

                solicitudesDemo.Add(solicitud);
            }

            await ctx.Solicitudes.AddRangeAsync(solicitudesDemo);
            await ctx.SaveChangesAsync();

            // 2) Generar TAREAS DEMO asociadas a algunas solicitudes aprobadas
            var solicitudesAprobadas = await ctx.Solicitudes
                .Where(s => s.Estado == EstadoS.Aprobada || s.Estado == EstadoS.Cerrada)
                .Take(150)
                .ToListAsync();

            var tareasDemo = new System.Collections.Generic.List<Tarea>();

            foreach (var sol in solicitudesAprobadas)
            {
                var fCreacion = sol.FechaCreacion.AddDays(rand.Next(0, 3)); // pocos días después
                var estadoT = (EstadoT)rand.Next(0, 4); // depende de tu enum EstadoT
                var prioridad = (Prioridad)rand.Next(0, 3); // Baja/Media/Alta, según tu enum

                DateTime? fechaLimite = fCreacion.AddDays(rand.Next(2, 10));
                DateTime? fechaCierre = null;
                TimeSpan tiempoInvertido = TimeSpan.Zero;

                if (estadoT == EstadoT.Resuelta)
                {
                    var diasResolucion = rand.Next(1, 15);
                    fechaCierre = fCreacion.AddDays(diasResolucion);
                    tiempoInvertido = TimeSpan.FromHours(rand.Next(1, 20));
                }

                var tarea = new Tarea
                {
                    Titulo = $"Tarea demo para {sol.Titulo}",
                    Descripcion = "Tarea generada para fines de demo del dashboard.",
                    FechaCreacion = fCreacion,
                    CategoriaId = categorias[rand.Next(categorias.Count)],
                    TipoServicioId = null, // o pon alguno si tienes
                    DivisionId = sol.DivisionId,
                    UnidadId = sol.UnidadId,
                    TecnicoId = tecnicos[rand.Next(tecnicos.Count)],
                    UsuarioId = sol.UsuarioId,
                    FechaLimite = fechaLimite,
                    Archivada = false,
                    FechaAsignacion = fCreacion,
                    FechaActualizacion = fechaCierre ?? fCreacion.AddDays(1),
                    Estado = estadoT,
                    Prioridad = prioridad,
                    TiempoInvertido = tiempoInvertido,
                    InicioContador = null,
                    SolicitudId = sol.Id,
                    FechaCierre = fechaCierre,
                    CausaRaiz = fechaCierre.HasValue ? "Causa raíz de prueba." : null,
                    PasosEjecutados = fechaCierre.HasValue ? "Pasos de resolución de prueba." : null,
                    AjustesRealizados = null,
                    ResultadoFinal = fechaCierre.HasValue ? "Incidencia resuelta." : null,
                    Recomendaciones = null,
                    UsuarioValidaId = null,
                    FechaValidacion = null,
                    MotivoPendiente = null
                };

                tareasDemo.Add(tarea);
            }

            await ctx.Tareas.AddRangeAsync(tareasDemo);
            await ctx.SaveChangesAsync();
        }
    }
}
