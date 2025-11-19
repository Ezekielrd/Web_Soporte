namespace DGASoporte.Models
{
    public class TareaAsignarVM
    {
        public Tarea Tarea { get; set; } = null!;

        public IEnumerable<Tecnico> Tecnicos { get; set; } = new List<Tecnico>();

        public IEnumerable<Nivel> Niveles { get; set; } = new List<Nivel>();
    }
}
