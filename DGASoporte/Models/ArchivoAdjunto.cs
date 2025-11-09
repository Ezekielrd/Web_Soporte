using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DGASoporte.Models
{
    public class ArchivoAdjunto
    {
        [Key]
        public int Id { get; set; }

        // Clave Foránea (FK) que enlaza a la Solicitud
        public int SolicitudId { get; set; }

        // Propiedad de navegación (para que EF Core sepa la relación)
        [JsonIgnore] //evitar referencias circulares
        public Solicitud Solicitud { get; set; } = null!;

        // Ruta donde se guarda el archivo en el servidor o la nube
        [Required]
        public string RutaArchivo { get; set; } = string.Empty;

        public string NombreOriginal { get; set; } = string.Empty;

        public string TipoMime { get; set; } = string.Empty; // Ej: "image/jpeg", "application/pdf"
    }
}
