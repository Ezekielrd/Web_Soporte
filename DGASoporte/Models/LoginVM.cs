using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models
{
    public class LoginVM
    {
        [StringLength(50, MinimumLength = 3)]
        [Required(ErrorMessage = "Ingrese su Usuario.")]
        [Display(Name = " Tu Usuario o correo")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Display(Name = "Tu Contraseña")]
        [DataType(DataType.Password)]
        [StringLength(50)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.,;#\-_=+]).{8,}$",
            ErrorMessage = "Mínimo 8 caracteres e incluir: mayúscula, minúscula, número y carácter especial.")]
        [Required(ErrorMessage = "Ingrese su Contraeña")]

        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; } = false;

        // Para redirigir después de iniciar sesión
        public string? ReturnUrl { get; set; }
    }
}
