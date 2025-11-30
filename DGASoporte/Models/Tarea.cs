using DGASoporte.Models.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DGASoporte.Models
{
    [Table("Tarea")]
    public class Tarea
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Titulo { get; set; } = string.Empty;
        [Required]
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        [Required]
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = default!;
        public int? TipoServicioId { get; set; }
        public TipoServicio TipoServicio { get; set; } = default!;
        public int? DivisionId { get; set; }
        public Division Division { get; set; } = default!;
        [Required]
        public int UnidadId { get; set; }
        public Unidad Unidad { get; set; } = default!;  
        public int? TecnicoId { get; set; }
        public Tecnico Tecnico { get; set; } = default!;
        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = default!;
        public DateTime? FechaLimite { get; set; }
        public bool Archivada { get; set; } = false;
        public DateTime? FechaAsignacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        [Required]
        public EstadoT? Estado {  get; set; } = default!;
        [Required]
        public Prioridad Prioridad  { get; set; } = default!;
        [Timestamp]
        public byte[]? RowVersion { get; set; }
        public TimeSpan TiempoInvertido { get; set; }
        public DateTime? InicioContador { get; set; }
        public int? SolicitudId { get; set; }
        //capos para solucion
        public DateTime? FechaCierre { get; set; }
        [MaxLength(1000)]
        public string? CausaRaiz { get; set; }
        [MaxLength(2000)]
        public string? PasosEjecutados { get; set; }
        [MaxLength(1500)]
        public string? AjustesRealizados { get; set; }
        [MaxLength(500)]
        public string? ResultadoFinal { get; set; }
        [MaxLength(1000)]
        public string? Recomendaciones { get; set; }
        public int? UsuarioValidaId { get; set; }
        public Usuario? UsuarioValida { get; set; }
        public DateTime? FechaValidacion { get; set; }

        // No mapeadas: útiles para UI
        [NotMapped]
        public bool Vencida => !Archivada && DateTime.Now.Date > FechaLimite?.Date;
        [NotMapped]
        public string Resumen =>
            string.IsNullOrWhiteSpace(Descripcion)
                ? Titulo
                : (Descripcion!.Length <= 120 ? Descripcion! : Descripcion!.Substring(0, 117) + "...");
        [NotMapped]
        public TimeSpan? TiempoResolucion =>
            FechaCierre.HasValue
                ? FechaCierre.Value - FechaCreacion
                : (TimeSpan?)null;
        public ICollection<Comentario> Historial { get; set; } = new List<Comentario>();
        public virtual ICollection<Diagnostico> Diagnosticos { get; set; } = new List<Diagnostico>();

    }
}
