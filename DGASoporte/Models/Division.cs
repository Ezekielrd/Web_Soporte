using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class Division
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; } = string.Empty;
        public ICollection<Unidad> Unidades { get; set; } = new List<Unidad>();

    }
}
