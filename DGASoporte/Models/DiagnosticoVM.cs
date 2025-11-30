using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class DiagnosticoVM
    {
        [Required]
        public int TareaId { get; set; }

        [Required(ErrorMessage = "El diagnóstico es obligatorio.")]
        [StringLength(2000, ErrorMessage = "El diagnóstico no puede superar los {1} caracteres.")]
        [Display(Name = "Descripción del diagnóstico")]
        public string Texto { get; set; } = string.Empty;

        [StringLength(2000)]
        [Display(Name = "Acciones recomendadas / solución propuesta")]
        public string? AccionesPropuestas { get; set; }

        [StringLength(2000)]
        [Display(Name = "Comentarios adicionales")]
        public string? ComentariosAdicionales { get; set; }

        [Display(Name = "Código de Equipo")]
        [StringLength(50, ErrorMessage = "El código no puede superar los {1} caracteres.")]
        public string? Codigo { get; set; }
    }
}
