using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DGASoporte.Models
{
    public class Diagnostico
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TareaId { get; set; }

        [ForeignKey(nameof(TareaId))]
        public virtual Tarea Tarea { get; set; } = null!;

        [Required]
        public int TecnicoId { get; set; }

        [ForeignKey(nameof(TecnicoId))]
        public virtual Tecnico Tecnico { get; set; } = null!;

        [Required]
        [StringLength(2000)]
        public string Texto { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? AccionesPropuestas { get; set; }

        [StringLength(2000)]
        public string? ComentariosAdicionales { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public string? Codigo { get; set; }
    }
}

