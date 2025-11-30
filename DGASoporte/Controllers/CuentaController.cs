
using DGASoporte.Data;         
using DGASoporte.Models;        
using DGASoporte.Seguridad;    
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DGASoporte.Controllers
{
    [AllowAnonymous]
    public class CuentaController : Controller
    {
        private readonly DGADbContext _context;
        private readonly ILogger<CuentaController> _logger;

        public CuentaController(DGADbContext context, ILogger<CuentaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Cuenta/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginVM { ReturnUrl = returnUrl });
        }

        // POST: /Cuenta/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
        {

            var entrada = (vm.UserNameOrEmail ?? "").Trim().ToLowerInvariant();
            var pwd = (vm.Password ?? "").Trim();
            // Si vienen vacíos -> mensaje propio
            if (string.IsNullOrWhiteSpace(entrada) || string.IsNullOrWhiteSpace(pwd))
            {
                ViewBag.Error = "Usuario o contraseña obligatorios.";
                return View(vm);
            }
            // Validaciones de DataAnnotations, etc.
            if (!ModelState.IsValid)
                return View(vm);

            bool esEmail = entrada.Contains('@');

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    esEmail ? u.Email.ToLower() == entrada : u.Usher.ToLower() == entrada);

            // Usuario no existe / inactivo / bloqueado -> mensaje propio
            if (usuario is null || !usuario.Activo || usuario.Bloqueado)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View(vm);
            }
            const int MAX_INTENTOS = 5;
            const int MINUTOS_BLOQUEO = 15;

            // Si está bloqueado por tiempo
            if (usuario.finBloqueo != null && usuario.finBloqueo > DateTime.Now)
            {
                ViewBag.Error = "Tu cuenta está bloqueada temporalmente. Intenta más tarde.";
                return View(vm);
            }
            var ok = PasswordHasher.Verificar(pwd, usuario.PasswordHash, usuario.PasswordSalt);
            if (!ok)
            {
                usuario.AccesoFallado++;

                if (usuario.AccesoFallado >= MAX_INTENTOS)
                {
                    usuario.finBloqueo = DateTime.Now.AddMinutes(MINUTOS_BLOQUEO);
                    //usuario.Bloqueado = true;
                }

                await _context.SaveChangesAsync();

                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View(vm);
            }
            usuario.AccesoFallado = 0;
            usuario.finBloqueo = null;
         
            // Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Usher),
                new Claim(ClaimTypes.Email, usuario.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var props = new AuthenticationProperties
            {
                IsPersistent = vm.RememberMe,
                AllowRefresh = true,
                ExpiresUtc = vm.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)   // recuerdame → 7 días
                    : DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

            // Redirección segura
            var destino = returnUrl ?? vm.ReturnUrl;
            if (!string.IsNullOrWhiteSpace(destino) && Url.IsLocalUrl(destino))
                return Redirect(destino);

            // Redirige según el rol
            return RedirectByRole(usuario.Rol?.Nombre);
        }

        // GET: /Cuenta/Logout
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // GET: /Cuenta/Denied
        [HttpGet]
        public IActionResult Denied()
        {
            return View();
        }

        private IActionResult RedirectByRole(string? roleName)
        {
            var rol = (roleName ?? string.Empty).Trim().ToLowerInvariant();

            return rol switch
            {
                "admin" => RedirectToAction("Index", "Home"),
                "tecnico" => RedirectToAction("Index", "Tecnico"),
                "cliente" => RedirectToAction("Index", "Solicitud"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}
