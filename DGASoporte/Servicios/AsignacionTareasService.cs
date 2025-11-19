namespace DGASoporte.Servicios
{
    using DGASoporte.Data;   // tu DbContext
    using DGASoporte.Hubs;
    using DGASoporte.Models; // Tarea, Tecnico, EstadoT, etc.
    using DGASoporte.Models.Enumeradores;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;

    public class AsignacionTareasService
    {
        private readonly DGADbContext _context;
        private readonly IHubContext<NotificacionesHub> _notificacionesHub;

        public AsignacionTareasService(DGADbContext context, IHubContext<NotificacionesHub> notificacionesHub)
        {
            _context = context;
            _notificacionesHub = notificacionesHub;

        }
        // 🔹 Helper: contar tareas activas de un técnico
        private int ContarTareasActivas(Tecnico t)
        {
            return t.TareasAsignadas
                .Count(ta => ta.Estado != EstadoT.Resuelta && !ta.Archivada);
        }

        // 🔹 Helper: recalcular disponibilidad (Disponible = true si no tiene tareas activas)
        public async Task ActualizarDisponibilidadTecnicoAsync(int tecnicoId)
        {
            var tecnico = await _context.Tecnicos
                .Include(t => t.TareasAsignadas)
                .FirstOrDefaultAsync(t => t.Id == tecnicoId);

            if (tecnico == null) return;

            var activas = ContarTareasActivas(tecnico);
            tecnico.Disponible = activas == 0;

            _context.Tecnicos.Update(tecnico);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Selecciona el técnico al que se le asignará una tarea.
        /// Primero busca entre disponibles (sin tareas activas).
        /// Si no hay, busca el menos cargado de todos.
        /// </summary>
        public async Task<Tecnico?> ObtenerTecnicoParaAsignarAsync()
        {
            // 1️⃣ Técnicos disponibles
            var tecnicoDisponible = await _context.Tecnicos
                .Include(t => t.TareasAsignadas)
                .Where(t => t.Disponible)
                .Select(t => new
                {
                    Tecnico = t,
                    TareasActivas = ContarTareasActivas(t)
                })
                .OrderBy(x => x.TareasActivas)
                .ThenBy(x => x.Tecnico.NivelId)
                .ThenBy(x => x.Tecnico.Id)
                .Select(x => x.Tecnico)
                .FirstOrDefaultAsync();

            if (tecnicoDisponible != null)
                return tecnicoDisponible;

            // 2️⃣ Si no hay disponibles → menos cargado de todos
            var tecnicoFallback = await _context.Tecnicos
                .Include(t => t.TareasAsignadas)
                .Select(t => new
                {
                    Tecnico = t,
                    TareasActivas = ContarTareasActivas(t)
                })
                .OrderBy(x => x.TareasActivas)
                .ThenBy(x => x.Tecnico.NivelId)
                .ThenBy(x => x.Tecnico.Id)
                .Select(x => x.Tecnico)
                .FirstOrDefaultAsync();

            return tecnicoFallback;
        }

        /// <summary>
        /// Busca la siguiente tarea libre (sin técnico), ordenada por prioridad y antigüedad.
        /// </summary>
        private async Task<Tarea?> ObtenerSiguienteTareaLibreAsync()
        {
            return await _context.Tareas
                .Where(t =>
                    !t.Archivada &&
                    t.TecnicoId == null &&
                    t.Estado == EstadoT.Nuevo) // ajusta el estado si usas otro para "pendiente"
                .OrderByDescending(t => t.Prioridad)   // mayor prioridad primero
                .ThenBy(t => t.FechaCreacion)          // luego la más antigua
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Evento 1: Botón "Asignar automático" en la lista (tarea específica).
        /// </summary>
        public async Task<bool> AsignarAutoPorTareaAsync(int tareaId)
        {
            var tarea = await _context.Tareas
                .FirstOrDefaultAsync(t => t.Id == tareaId);

            if (tarea == null)
                return false;

            // Ya tiene técnico → no tocar
            if (tarea.TecnicoId.HasValue)
                return false;

            var tecnico = await ObtenerTecnicoParaAsignarAsync();
            if (tecnico == null)
                return false;

            tarea.TecnicoId = tecnico.Id;
            tarea.FechaAsignacion = DateTime.Now;
            tarea.Estado = EstadoT.Asignado;
            tarea.FechaActualizacion = DateTime.Now;

            _context.Tareas.Update(tarea);
            _context.Asignaciones.Add(new Asignacion
            {
                TareaId = tarea.Id,
                TecnicoId = tarea.TecnicoId.Value,
                Fecha = DateTime.Now,
                Modo = "Manual",
            });
            await _context.SaveChangesAsync();

            await ActualizarDisponibilidadTecnicoAsync(tecnico.Id);

            return true;
        }

        /// <summary>
        /// Evento 2: Opción C – asignar al técnico la siguiente tarea libre (cuando finaliza una).
        /// </summary>
        public async Task<Tarea?> AsignarSiguienteTareaATecnicoAsync(int tecnicoId)
        {
            var tecnico = await _context.Tecnicos
                .FirstOrDefaultAsync(t => t.Id == tecnicoId);

            if (tecnico == null)
                return null;

            var tarea = await ObtenerSiguienteTareaLibreAsync();
            if (tarea == null)
                return null;

            tarea.TecnicoId = tecnico.Id;
            tarea.FechaAsignacion = DateTime.Now;
            tarea.Estado = EstadoT.Asignado;
            tarea.FechaActualizacion = DateTime.Now;

            _context.Tareas.Update(tarea);
            await _context.SaveChangesAsync();

            return tarea;
        }

        /// <summary>
        /// Evento 3: Asignación manual desde tu botón "Asignar" con select de técnicos.
        /// </summary>
        public async Task<bool> AsignarManualAsync(int tareaId, int tecnicoId)
        {
            var tarea = await _context.Tareas
                .FirstOrDefaultAsync(t => t.Id == tareaId);
            var tecnico = await _context.Tecnicos
                .FirstOrDefaultAsync(t => t.Id == tecnicoId);

            if (tarea == null || tecnico == null)
                return false;

            tarea.TecnicoId = tecnico.Id;
            tarea.FechaAsignacion = DateTime.Now;
            tarea.Estado = EstadoT.Asignado;
            tarea.FechaActualizacion = DateTime.Now;

            _context.Tareas.Update(tarea);
            _context.Asignaciones.Add(new Asignacion
            {
                TareaId = tarea.Id,
                TecnicoId = tarea.TecnicoId.Value,
                Fecha = DateTime.Now,
                Modo = "Auto",
            });
            var evento = new
            {
                tareaId = tarea.Id,
                titulo = tarea.Titulo,
                tecnicoId = tecnico.Id,
                tecnicoNombre = tecnico.Usuario.NombreCompleto, // o propiedad real
                prioridad = tarea.Prioridad // si existe
            };

            // Por ahora enviamos a TODOS los clientes.
            await _notificacionesHub.Clients.All.SendAsync("TaskAssigned", evento);

            await _context.SaveChangesAsync();

            await ActualizarDisponibilidadTecnicoAsync(tecnico.Id);

            return true;
        }
    
    }
}
