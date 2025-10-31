using DGASoporte.Data;
using DGASoporte.Models;
using DGASoporte.Seguridad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DGASoporte.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly DGADbContext _context;

        private const int tecnicoid = 2;

        public UsuarioController(DGADbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q)
        {
           
            var buscar = _context.Usuarios
                .AsNoTracking()
                .Where(u => string.IsNullOrWhiteSpace(q) || EF.Functions.Like(u.User, $"%{q}%"));
            var usuarios = await buscar
                .Include(u => u.Rol)
                .OrderBy(u => u.User)
                .ToListAsync();

            ViewBag.Q = q ?? "";

            return View(usuarios);
        }
        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);
            var tec = await _context.Tecnicos
                .AsNoTracking()
                .Include(t => t.Nivel)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null) return NotFound();


            var vm = new UsuarioVM
            {
                User = usuario.User,
                Email = usuario.Email,
                NombreCompleto = usuario.NombreCompleto,
                Codigo = usuario.Codigo,
                RolId = usuario.RolId,
                Rol = usuario.Rol,   
                Activo = usuario.Activo,
                FechaCracion = usuario.CreadoEn,
                Nivel = tec?.Nivel
            };

            return View(vm);
        }

        // GET: Usuarios/Create
        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View(new UsuarioVM { Activo = true });
        }
        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioVM vm)
        {
            //Validaciones de duplicados
            if (await _context.Usuarios.AnyAsync(u => u.Email == vm.Email))
                ModelState.AddModelError(nameof(vm.Email), "El correo ya está registrado.");

            if (await _context.Usuarios.AnyAsync(u => u.User == vm.User))
                ModelState.AddModelError(nameof(vm.User), "El usuario ya existe.");

            if (await _context.Usuarios.AnyAsync(u => u.Codigo == vm.Codigo))
                ModelState.AddModelError(nameof(vm.Codigo), "El código ya existe.");

            // Validación si es técnico
            if (vm.RolId == tecnicoid)
            {
                if (vm.NivelId == null)
                    ModelState.AddModelError(nameof(vm.NivelId), "El Nivel es Obligatorio");
            }

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(); // repoblar selects como SelectListItem
                return View(vm);
            }

            //Hash + salt
            var (hash, salt) = PasswordHasher.Hash(vm.Password);

            //Construcción de entidad según rol
            if (vm.RolId == tecnicoid)
            {
                var tecnico = new Tecnico
                {
                    User = vm.User.Trim(),
                    Email = vm.Email.Trim(),
                    NombreCompleto = vm.NombreCompleto.Trim(),
                    Codigo = vm.Codigo.Trim(),
                    RolId = vm.RolId,
                    Activo = vm.Activo,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    CreadoEn = DateTime.UtcNow,
                    AccesoFallado = 0,
                    Bloqueado = false,
                    Disponible = false,
                    NivelId = vm.NivelId!.Value 
                };

                _context.Usuarios.Add(tecnico); // TPT: ok agregar derivada en DbSet base
            }
            else
            {
                var usuario = new Usuario
                {
                    User = vm.User.Trim(),
                    Email = vm.Email.Trim(),
                    NombreCompleto = vm.NombreCompleto.Trim(),
                    Codigo = vm.Codigo.Trim(),
                    RolId = vm.RolId,
                    Activo = vm.Activo,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    CreadoEn = DateTime.UtcNow,
                    AccesoFallado = 0,
                    Bloqueado = false
                };

                _context.Usuarios.Add(usuario);
            }

            //Guardar con manejo de errores
            try
            {
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Si hay FK/UNIQUE errores, mostrarlos
                ModelState.AddModelError(string.Empty, $"No se pudo guardar: {ex.GetBaseException().Message}");
                await CargarCombosAsync();
                return View(vm);
            }
        }
            // GET: Usuarios/Edit/5
            [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.Usuarios
             .AsNoTracking()
             .Include(x => x.Rol)
             .FirstOrDefaultAsync(x => x.Id == id);

            if (usuario == null) return NotFound();

            if (usuario.RolId == tecnicoid)
            {
                var nivles = await _context.Nivles
                .AsNoTracking()
                .OrderBy(n => n.Nombre)
                .Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Nombre })
                .ToListAsync();
            }
                var tecnico = await _context.Tecnicos
               .AsNoTracking()
               .Include(x => x.Nivel)
               .FirstOrDefaultAsync(x => x.Id == id);

            var roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Nombre)
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre })
                .ToListAsync();
            var vm = new UsuarioVM
            {
                Id = usuario.Id,
                User = usuario.User,
                Email = usuario.Email,
                NombreCompleto = usuario.NombreCompleto,
                RolId = usuario.RolId,
                Codigo = usuario.Codigo,
                Roles = roles,
                Activo = usuario.Activo,
                NivelId = tecnico?.NivelId               
            };

            ViewBag.Roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
            return View(vm);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsuarioVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                ModelState.Remove(nameof(UsuarioVM.Password));
            }
            if (!ModelState.IsValid)
            {
                if (vm.RolId == tecnicoid)
                    ViewBag.Nivles = await _context.Nivles.OrderBy(n => n.Nombre).ToListAsync();
                ViewBag.Roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
                return View(vm);
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == vm.Id);
            if (usuario is null) return NotFound();

            // Validaciones de datso exixtentes
            if (await _context.Usuarios.AnyAsync(u => u.Email == vm.Email && u.Id != vm.Id))
                ModelState.AddModelError(nameof(vm.Email), "El correo ya está registrado por otro usuario.");
            if (await _context.Usuarios.AnyAsync(u => u.User == vm.User && u.Id != vm.Id))
                ModelState.AddModelError(nameof(vm.User), "El usuario ya existe en otra cuenta.");
            if (await _context.Usuarios.AnyAsync(u => u.Codigo == vm.Codigo))
                ModelState.AddModelError(nameof(vm.User), "El codigo ya existe.");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
                return View(vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();

            // Actualizar datos básicos
            usuario.User = vm.User.Trim();
            usuario.Email = vm.Email.Trim();
            usuario.NombreCompleto = vm.NombreCompleto.Trim();
            usuario.Codigo= vm.Codigo.Trim();
            usuario.RolId = vm.RolId;
            usuario.Activo = vm.Activo;

            // Cambiar contraseña SOLO si Password viene con valor
            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var (hash, salt) = PasswordHasher.Hash(vm.Password);
                usuario.PasswordHash = hash;
                usuario.PasswordSalt = salt;
                usuario.ActualizadoEn = DateTime.UtcNow;
            }
            try 
            {
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            } 
            catch (DbUpdateConcurrencyException)
            {
              if (!await _context.Usuarios.AnyAsync(e => e.Id == vm.Id)) return NotFound();
               throw;
            }
        }
        // GET: /Usuarios/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user is null) return NotFound();

            // Si lo llamas por AJAX para modal, devuelve parcial
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Delete", user); // modal

            return View(user); // vista completa
        }

        // POST: /Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user is null) return NotFound();

            //DELETE:
            _context.Usuarios.Remove(user);

            await _context.SaveChangesAsync();

            // Si vino de un modal por AJAX, puedes devolver 204 o JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return NoContent();

            TempData["Ok"] = "Usuario eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBloqueo(int id, DateTime? hasta = null)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            if (usuario.Bloqueado && (!usuario.finBloqueo.HasValue || usuario.finBloqueo > DateTime.UtcNow))
            {
                // Estaba bloqueado -> Desbloquear
                usuario.Bloqueado = false;
                usuario.finBloqueo = null;
                usuario.AccesoFallado = 0;
            }
            else
            {
                // Estaba desbloqueado -> Bloquear (temporal o indefinido)
                usuario.Bloqueado = true;
                usuario.finBloqueo = hasta ?? DateTime.UtcNow.AddYears(100);
            }

            usuario.ActualizadoEn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, [FromForm] string newPassword)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                TempData["Err"] = "La nueva contraseña es requerida y debe tener al menos 8 caracteres.";
                return RedirectToAction(nameof(Index));
            }

            // 1) Generar nuevo hash + sal
            var (hash, salt) = PasswordHasher.Hash(newPassword);

            // 2) Guardar en la entidad
            usuario.PasswordHash = hash;
            usuario.PasswordSalt = salt;

            // Limpia bloqueos/errores
            usuario.AccesoFallado = 0;
            usuario.finBloqueo = null;
            usuario.Bloqueado = false;
            usuario.ActualizadoEn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Msg"] = $"Contraseña de {usuario.User} restablecida correctamente.";
            return RedirectToAction(nameof(Index));
        }
        private async Task CargarCombosAsync()
        {
            ViewBag.Roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Nombre)
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre })
                .ToListAsync();

            ViewBag.Niveles = await _context.Nivles
                .AsNoTracking()
                .OrderBy(n => n.Nombre)
                .Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Nombre })
                .ToListAsync();
        }
    }
}
