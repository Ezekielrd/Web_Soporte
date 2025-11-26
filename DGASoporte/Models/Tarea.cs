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
        public int? UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = default!;
        public DateTime? FechaLimite { get; set; }
        public bool Archivada { get; set; } = false;
        public DateTime? FechaAsignacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        [Required]
        public EstadoT? Estado {  get; set; } = default!;
        [Required]
        public Prioridad Prioridad  { get; set; } = default!;
        public DateTime? FechaInicioDiagnostico { get; set; }
        public string? DiagnosticoInicial { get; set; }
        [Timestamp]
        public byte[]? RowVersion { get; set; }
        public TimeSpan TiempoInvertido { get; set; }
        public DateTime? InicioContador { get; set; }

        // No mapeadas: útiles para UI
        [NotMapped]
        public bool Vencida => !Archivada && DateTime.Now.Date > FechaLimite?.Date;
        [NotMapped]
        public string Resumen =>
            string.IsNullOrWhiteSpace(Descripcion)
                ? Titulo
                : (Descripcion!.Length <= 120 ? Descripcion! : Descripcion!.Substring(0, 117) + "...");
        public ICollection<Comentario> Historial { get; set; } = new List<Comentario>();

    }
}
