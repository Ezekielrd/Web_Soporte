using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DGASoporte.Models
{
    [Table("Tecnico")]
    public class Tecnico : Usuario
    {
        [Column("Disponible")]
        public bool Disponible { get; set; }

        [Column("Nivel")]
        public int Nivel { get; set; }

        // Navegación 1..N (Tecnico -> Tareas)
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}
