using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class Asignacion
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int TareaId { get; set; }
        [Required]
        public int TecnicoId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Modo { get; set; } = string.Empty; // "manual" o "auto"
    }
}
