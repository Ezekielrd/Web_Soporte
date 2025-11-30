using DGASoporte.Models.Enumeradores;
using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class Solicitud
    {
        [Key]
        public int Id { get; set; }

        // Datos Reportados por el Usuario
        [Required]
        [StringLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relaciones (Foreign Keys)
        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = default!;
        public int? DivisionId { get; set; }
        public Division Division { get; set; } = default!;
        [Required]
        public int UnidadId { get; set; }
        public Unidad Unidad { get; set; } = default!;
        [Required]
        public int TipoIncidenciaId { get; set; }
        public TipoIncidencia TipoIncidencia { get; set; } = default!;
        [Required]
        public EstadoS? Estado { get; set; }
        public string? MotivoRechazo { get; set; }
        public bool Archivada { get; set; } = false;
        public DateTime? FechaActualizacion { get; set; }

    }
}
