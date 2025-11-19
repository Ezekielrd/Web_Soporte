using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class TipoServicio
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(70)]
        public string Nombre { get; set; } = string.Empty;
        [StringLength(250)]
        public string? Descripcion { get; set; }
        [Required]
        public int CategoriaId { get; set; }

        public Categoria Categoria { get; set; } = default!;
    }
}
