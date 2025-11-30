using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class ComentarioVM
    {
        [Required]
        public int TareaId { get; set; }

        [Required(ErrorMessage = "El comentario es obligatorio.")]
        [StringLength(2000, ErrorMessage = "El comentario no puede superar los {1} caracteres.")]
        [MinLength(5, ErrorMessage = "El comentario debe tener al menos {1} caracteres.")]
        [Display(Name = "Comentario")]
        public string Texto { get; set; } = string.Empty;
    }
}
