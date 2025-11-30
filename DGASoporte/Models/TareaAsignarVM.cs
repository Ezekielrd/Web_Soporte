using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class TareaAsignarVM
    {
        public Tarea Tarea { get; set; } = default!;
        public List<Tecnico> Tecnicos { get; set; } = new();
        public List<Nivel> Niveles { get; set; } = new();

        [Display(Name = "Técnico")]
        [Required(ErrorMessage = "Debe seleccionar un técnico.")]
        public int? TecnicoId { get; set; }
    }
}
