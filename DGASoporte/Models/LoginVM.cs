using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class LoginVM
    {
        [Required, Display(Name = "Usuario")]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required, Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; } = false;

        // Para redirigir después de iniciar sesión
        public string? ReturnUrl { get; set; }
    }
}
