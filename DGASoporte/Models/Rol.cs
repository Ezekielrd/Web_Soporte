using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DGASoporte.Models
{
    [Table("Rol")]
    public class Rol
    {
        public int Id { get; set; }

        [Required, StringLength(60)]
        public string Nombre { get; set; } = null!; 

        [StringLength(200)]
        public string? Descripcion { get; set; }

        // relacion 1..N
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
