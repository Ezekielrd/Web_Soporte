using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }

        // Usuario destinatario
        public int UsuarioId { get; set; }

        // Tipo / categoría: "SolicitudCreada", "TareaAsignada", etc.
        public string Tipo { get; set; } = default!;

        // Texto corto que se ve en el listado
        public string Titulo { get; set; } = default!;
        public string Mensaje { get; set; } = default!;

        // A dónde lo llevo si hace clic
        public string? UrlDestino { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public bool Leida { get; set; } = false;
    }

}
