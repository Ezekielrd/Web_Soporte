using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class PausaVM
    {
        [Required]
        public int TareaId { get; set; }

        [Required(ErrorMessage = "Debe indicar el motivo de la pausa.")]
        [StringLength(1000, ErrorMessage = "El motivo no puede superar los {1} caracteres.")]
        [MinLength(10, ErrorMessage = "El motivo debe tener al menos {1} caracteres.")]
        [Display(Name = "Motivo de la pausa")]
        public string Motivo { get; set; } = string.Empty;
    }
}
