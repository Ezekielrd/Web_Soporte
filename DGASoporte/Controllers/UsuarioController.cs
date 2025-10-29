using DGASoporte.Data;
using DGASoporte.Models;
using DGASoporte.Seguridad;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DGASoporte.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly DGADbContext _context;
        public UsuarioController(DGADbContext context) => _context = context;

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
        // GET: /Users/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(u=>u.Id == id);
            if (user is null) return NotFound();

            var vm = new UsuarioVM
            {
                Id = user.Id,
                User = user.User ?? "",
                Email = user.Email,
                Activo = user.Activo,
                FechaCracion = user.CreadoEn,
                Rol = user.Rol?.Nombre
            };

            return View(vm);
        }

        // GET: UsuarioController/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Nombre)
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre })
                .ToListAsync();
            return View(new UsuarioVM());
        }
        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioVM vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
                return View(vm);
            }

            // Validaciones de datos existentes
            if (await _context.Usuarios.AnyAsync(u => u.Email == vm.Email))
                ModelState.AddModelError(nameof(vm.Email), "El correo ya está registrado.");
            if (await _context.Usuarios.AnyAsync(u => u.User == vm.User))
                ModelState.AddModelError(nameof(vm.User), "El usuario ya existe.");
            if (await _context.Usuarios.AnyAsync(u => u.Codigo == vm.Codigo))
                ModelState.AddModelError(nameof(vm.User), "El codigo ya existe.");


            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
                return View(vm);
            }

            // Hash + salt
            var (hash, salt) = PasswordHasher.Hash(vm.Password);

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
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null) return NotFound();

            var vm = new UsuarioVM
            {
                Id = usuario.Id,
                User = usuario.User,
                Email = usuario.Email,
                NombreCompleto = usuario.NombreCompleto,
                RolId = usuario.RolId,
                Activo = usuario.Activo
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

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
                return View(vm);
            }

            // Actualizar datos básicos
            usuario.User = vm.User.Trim();
            usuario.Email = vm.Email.Trim();
            usuario.NombreCompleto = vm.NombreCompleto?.Trim();
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
                TempData["Err"] = "La nueva contraseña es requerida y debe tener al menos 6 caracteres.";
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

    }
}
