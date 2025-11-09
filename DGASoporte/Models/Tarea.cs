using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DGASoporte.Models
{
    [Table("Tarea")]
    public class Tarea
    {
        [Key]
        public int Id { get; set; }

        [Column("Titulo")]
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Column("Descripcion")]
        [StringLength(4000)]
        public string? Descripcion { get; set; }

        [Column("FechaCreacion", TypeName = "DATETIME")]
        [Display(Name = "Creada")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Column("EstadoId")]
        [Display(Name = "Estado")]
        [Required]
        public int EstadoId { get; set; }
        public Estado Estado { get; set; } = default!;

        [Column("PrioridadId")]
        [Display(Name = "Prioridad")]
        [Required]
        public int PrioridadId { get; set; }
        public Prioridad Prioridad { get; set; } = default!;

        [Column("CategoriaId")]
        [Display(Name = "Categoría")]
        public int? CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = default!;

        [Column("UnidadId")]
        [Display(Name = "Unidad")]
        [Required]
        public int UnidadId { get; set; }

        public Unidad Unidad { get; set; } = default!;

        [Column("TecnicoId")]
        [Display(Name = "Tecnico")]
        [Required]
        public int TecnicoId { get; set; }
        public Tecnico Tecnico { get; set; } = default!;

        [Column("FechaLimite", TypeName = "DATETIME")]
        [Display(Name = "Fecha Límite")]
        [Required(ErrorMessage = "La fecha límite es obligatoria")]
        public DateTime FechaLimite { get; set; }

        [Display(Name = "Archivada")]
        public bool Archivada { get; set; } = false;
        public DateTime? FechaAsignacion { get; set; }


        [Display(Name = "Última actualización")]
        public DateTime? FechaActualizacion { get; set; }

        // Recomendado: columna de concurrencia (si puedes agregarla en BD como rowversion)
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // No mapeadas: útiles para UI
        [NotMapped]
        public bool Vencida => !Archivada && DateTime.Now.Date > FechaLimite.Date;

        [NotMapped]
        public string Resumen =>
            string.IsNullOrWhiteSpace(Descripcion)
                ? Titulo
                : (Descripcion!.Length <= 120 ? Descripcion! : Descripcion!.Substring(0, 117) + "...");
    }
}
