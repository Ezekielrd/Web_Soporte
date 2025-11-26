using DGASoporte.Models;
using DGASoporte.Models.Enumeradores;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class TareaFormVM
{
    public int Id { get; set; }

    [StringLength(100, ErrorMessage = "El título no puede superar los 100 caracteres")]
    [Display(Name = "Titulo de Tarea")]
    [Required(ErrorMessage ="El Titulo es Requerido")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(800)]
    [Display(Name = "Descripcion de la Tarea")]
    [Required(ErrorMessage = "La Descriprcion es Requerido")]
    public string Descripcion { get; set; } = string.Empty;

    [Display(Name = "Fecha Creacion")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Fecha Límite")]
    [DataType(DataType.Date)]
    public DateTime? FechaLimite { get; set; }

    [Display(Name = "Fecha Actualizacion")]
    public DateTime? FechaActualizacion { get; set; }

    [Display(Name = "Fecha Asignacion Tecnico")]
    public DateTime? FechaAsignacion { get; set; }

    [Display(Name = "Categorías")]
    [Required(ErrorMessage = "Debe elegir la Categoria")]
    public int CategoriaId { get; set; }
    public string? CategoriaNombre { get; init; }
    [Display(Name = "Tipo Servicios")]
    public int? TipoServicioId { get; set; }
    public string? TipoServicioNombre { get; init; }
    public Dictionary<int, string> MapTipoDesc { get; set; } = new();

    [Display(Name = "Areas")]
    [Required(ErrorMessage = "Debe eligir el Area")]
    public int UnidadId { get; set; }
    public string? UnidadNombre { get; init; }
    [Display(Name = "Divisiones")]
    public int? DivisionId { get; set; }
    public string? DivisionNombre { get; init; }

    [Display(Name = "Tecnicos")]
    public int? TecnicoId { get; set; }
    public string? TecnicoNombre { get; init; }
    [Display(Name = "Usuario Solicitante")]
    public int UsuarioId { get; set; }
    public string? UsuarioNombre { get; init; }
    public bool Archivada { get; set; }
    [Display(Name = "Estados")]

    public EstadoT? Estado { get; set; }
    public string? EstadoNombre { get; init; }

    [Display(Name = "Prioridades")]
    [Required(ErrorMessage = "Debe elegir la Prioridad")]
    public Prioridad Prioridad { get; set; }
    public string? PrioridadNombre { get; init; }

    public TimeSpan TiempoInvertido { get; set; }
    public DateTime? InicioContador { get; set; }
    public DateTime? FechaInicioDiagnostico { get; set; }
    [Display(Name = "Diagnostico Inicial")]
    [Required(ErrorMessage = "Debe realizar el Diagnistico")] 
    public string DiagnosticoInicial { get; set; } = string.Empty;

    // Para concurrencia (si la usas)
    public byte[]? RowVersion { get; set; }

    // SelectLists
    public IEnumerable<SelectListItem> Categorias { get; set; } = [];
    public IEnumerable<SelectListItem> Tecnicos { get; set; } = [];
    public List<TipoServicio> TiposServicio { get; set; } = new();
    public IEnumerable<SelectListItem> Divisiones { get; set; } = Enumerable.Empty<SelectListItem>();
    public List<Unidad> Unidades { get; set; } = new();
    public ICollection<Comentario> Historial { get; set; } = new List<Comentario>();

}
