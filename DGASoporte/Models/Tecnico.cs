using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DGASoporte.Models
{
    [Table("Tecnico")]
    public class Tecnico : Usuario
    {
        [Column("Disponible")]
        public bool Disponible { get; set; }
        public int NivelId { get; set; }
        public Nivel Nivel { get; set; } = default!;

        // Navegación 1..N (Tecnico -> Tareas)
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}
