using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DGASoporte.Models
{
    [Table("Division")]
    public class Division
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;
        public ICollection<Unidad> Tareas { get; set; } = new List<Unidad>();
    }
}
