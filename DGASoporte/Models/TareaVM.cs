using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TareaFormVM
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Descripcion { get; set; }
    [Display(Name = "Fecha Creacion")]
    public DateTime FechaCreacion { get; init; }
    [Display(Name = "Fecha Límite")]
    [Required]
    public DateTime FechaLimite { get; set; } = DateTime.Now.AddDays(7);

    [Display(Name = "Estado"), Required]
    public int EstadoId { get; set; }
    public string? EstadoNombre { get; init; }

    [Display(Name = "Prioridad"), Required]
    public int PrioridadId { get; set; }
    public string? PrioridadNombre { get; init; }

    [Display(Name = "Categoría")]
    public int? CategoriaId { get; set; }
    public string? CategoriaNombre { get; init; }

    [Display(Name = "Unidad"), Required]
    public int UnidadId { get; set; }
    public string? UnidadNombre { get; init; }

    public bool Archivada { get; set; }

    // Para concurrencia (si la usas)
    public byte[]? RowVersion { get; set; }

    // SelectLists
    public IEnumerable<SelectListItem> Estados { get; set; } = [];
    public IEnumerable<SelectListItem> Prioridades { get; set; } = [];
    public IEnumerable<SelectListItem> Categorias { get; set; } = [];
    public IEnumerable<SelectListItem> Unidades { get; set; } = [];
}
