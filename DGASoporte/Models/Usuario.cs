using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DGASoporte.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string User { get; set; } = null!;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = null!;

        [Required, StringLength(160)]
        public string NombreCompleto { get; set; } = "";

        [Required, StringLength(50)]
        public string Codigo { get; set; } = null!;

        // Hashear la contraseña, no guardar texto plano
        [Required, StringLength(250)]
        public string PasswordHash { get; set; } = null!;
        [Required]
        public string PasswordSalt { get; set; } = default!;
        public bool Activo { get; set; } = true;
        public bool Bloqueado { get; set; }

        // Control de bloqueos básicos
        public int AccesoFallado { get; set; } = 0;
        public DateTime? finBloqueo { get; set; }

        // FK
        public int RolId { get; set; }

        // Relacion
        public Rol? Rol { get; set; }

        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? ActualizadoEn { get; set; }
        public DateTime? UltimoIngreso { get; set; }
    }
}
