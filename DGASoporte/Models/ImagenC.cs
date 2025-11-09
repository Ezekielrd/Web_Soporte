using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DGASoporte.Models
{
    public class ImagenC
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public byte[] DatosBinarios { get; set; }
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; }= string.Empty;
        
        [MaxLength(250)]
        public string Descripcion { get; set; }=string.Empty;
        [Required]
        public int ComentarioId { get; set; }
        [JsonIgnore]
        public Comentario Comentario { get; set; }
    }
}
