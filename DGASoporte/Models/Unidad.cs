using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;

namespace DGASoporte.Models
{
    [Table("Unidad")]
    public class Unidad
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;
        [Column("DivisionId")]
        public int DivisionId { get; set; }
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}
