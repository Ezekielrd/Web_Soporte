using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class UsuarioVM
    {
        public int? Id { get; set; }

        [StringLength(100)]
        [Display(Name = "Nombre Usuario")]
        [Required(ErrorMessage = "El Nombre es Obligatorio")]
        public string User { get; set; } = null!;
        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "Correo Usuario")]
        [Required(ErrorMessage = "El Correo es Obligatorio")]
        public string Email { get; set; } = null!;

        [StringLength(150)]
        [Display(Name = "Nombre Completo")]
        [Required(ErrorMessage = "El NombreCompleto es Obligatorio")]
        public string NombreCompleto { get; set; } = string.Empty!;
        [StringLength(50)]
        [Display(Name = "Codigo")]
        [Required(ErrorMessage = "El Codigo es Obligatorio")]
        public string Codigo { get; set; } = string.Empty!;

        public DateTime FechaCracion { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        [Display(Name = "Contraseña Usuario")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.,;#\-_=+]).{8,}$",
            ErrorMessage = "Mínimo 8 caracteres e incluir: mayúscula, minúscula, número y carácter especial.")]
        [Required(ErrorMessage = "La Contraseña es Obligatorio")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Seleccione un rol")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un rol válido")]
        [Display(Name = "Rol")]
        public int RolId { get; set; }
        public Rol? Rol { get; init; }

        public bool Activo { get; set; } = true;
        public bool Bloqueado { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; } = [];
    }
}
