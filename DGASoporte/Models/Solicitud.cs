using DGASoporte.Models.Enumeradores;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public int UsuarioId { get; set; } // FK al usuario que reporta
        public Usuario Usuario { get; set; } = default!;
        [Required]
        public int UnidadId { get; set; }

        public Unidad Unidad { get; set; } = default!;
        public TipoIncidencia Tipo { get; set; }

        // Relación de Uno a Muchos con Archivos Adjuntos
        public ICollection<ArchivoAdjunto> ArchivosAdjuntos { get; set; } = new List<ArchivoAdjunto>();
    }
}
