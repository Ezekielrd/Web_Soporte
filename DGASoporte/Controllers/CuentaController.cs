
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
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginVM { ReturnUrl = returnUrl });
        }

        // POST: /Cuenta/Login
        [HttpPost]
        [AllowAnonymous]
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
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // GET: /Cuenta/Denied
        [HttpGet]
        [AllowAnonymous]
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

        // ========= Método semilla para Admin =========
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SeedAdmin()
        {
            var rolAdmin = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Admin");
            if (rolAdmin == null)
            {
                rolAdmin = new Rol { Nombre = "Admin"};
                _context.Roles.Add(rolAdmin);
                await _context.SaveChangesAsync();
            }

            var usher = "admi";
            var email = "admin@demo.com";

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.Usher.ToLower() == usher.ToLower() || u.Email.ToLower() == email.ToLower());

            var passPlano = "Admin123*";
            var (hash, salt) = PasswordHasher.Hash(passPlano);

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    Usher = usher,
                    Email = email,
                    NombreCompleto = "Administrador del sistema",
                    Codigo = "ADM-001",
                    Activo = true,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    RolId = rolAdmin.Id,
                    Bloqueado = false
                };
                _context.Usuarios.Add(usuario);
            }
            else
            {
                usuario.PasswordHash = hash;
                usuario.PasswordSalt = salt;
                usuario.Activo = true;
                usuario.RolId = rolAdmin.Id;
                usuario.Bloqueado = false;
            }

            await _context.SaveChangesAsync();

            // ✅ verificación directa en memoria
            var okMem = PasswordHasher.Verificar(passPlano, hash, salt);
            return Content($"Admin listo (Usher={usuario.Usher}). Verificación hasher: {okMem}");
        }
    }
}
