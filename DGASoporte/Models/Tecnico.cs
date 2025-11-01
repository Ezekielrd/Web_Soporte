using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DGASoporte.Models
{
    [Table("Tecnico")]
    public class Tecnico
    {
        [Key]
        public int Id { get; set; }
        [Column("Disponible")]
        public bool Disponible { get; set; }
        public int NivelId { get; set; }
        public Nivel Nivel { get; set; } = default!;

        [ForeignKey(nameof(Id))]
        public Usuario Usuario { get; set; } = null!;

        // Navegación 1..N (Tecnico -> Tareas)
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}
