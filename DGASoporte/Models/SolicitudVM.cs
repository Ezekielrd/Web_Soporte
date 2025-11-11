using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class SolicitudVM
    {
        [Key]
        public int Id { get; set; }

        // Datos Reportados por el Usuario
        [Display(Name = "Titulo de la Incidencia")]
        [Required(ErrorMessage = "El Título es Obligatorio.")]
        [StringLength(100)]
        public string Titulo { get; set; } = string.Empty;
        [Display(Name = "Descricpcion de la Incidencia")]
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relaciones (Foreign Keys)
        [Required]
        public int UsuarioId { get; set; } 
        public string UsuarioNombre { get; set; } = default!;

        [Display(Name = "Area  de Procedencia")]
        [Required(ErrorMessage = "La Unidad es obligatoria.")]

        public int UnidadId { get; set; }

        public string? UnidadNombre { get; set; } = default!;
        public IEnumerable<SelectListItem> Unidades { get; set; } = [];
        [Required(ErrorMessage = "El TipoIncidencia es obligatoria.")]
        [Display(Name = "Tipo de Incidencia")]

        public int TipoIncidenciaId { get; set; }

        public string? TipoIncidenciaNombre { get; set; } = default!;
        public IEnumerable<SelectListItem> TipoIncidencias { get; set; } = [];

    }
}
