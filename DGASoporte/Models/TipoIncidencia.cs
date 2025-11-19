using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class TipoIncidencia
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; } = string.Empty;
        [StringLength(250)]                 
        public string? Descripcion { get; set; }
    }
}
