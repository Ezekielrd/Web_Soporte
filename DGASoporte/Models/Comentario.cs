using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DGASoporte.Models
{
    public class Comentario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Texto { get; set; } = string.Empty;

        [Required]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; } = null!;

        [Required]
        public int TareaId { get; set; }
        public virtual Tarea Tarea { get; set; } = null!;

    }
}
