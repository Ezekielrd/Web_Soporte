using DGASoporte.Data;
using DGASoporte.Models;
using DGASoporte.Seguridad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

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
                .Where(u => string.IsNullOrWhiteSpace(q) || EF.Functions.Like(u.Usher, $"%{q}%"));
            var usuarios = await buscar
                .Include(u => u.Rol)
                .OrderBy(u => u.Usher)
                .ToListAsync();

            ViewBag.Q = q ?? "";

            return View(usuarios);
        }
        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                TempData["Alert"] = "El Identificador no es Válido";
                return RedirectToAction(nameof(Index));
            }
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
                Usher = usuario.Usher,
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
            var vm = new UsuarioVM { Activo = true };
            await CargarCombosAsync(vm);
            return View(vm);
        }
        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioVM vm)
        {
            //Validaciones de duplicados
            if (await _context.Usuarios.AnyAsync(u => u.Email == vm.Email))
                ModelState.AddModelError(nameof(vm.Email), "El correo ya está registrado.");

            if (await _context.Usuarios.AnyAsync(u => u.Usher == vm.Usher))
                ModelState.AddModelError(nameof(vm.Usher), "El usuario ya existe.");

            if (await _context.Usuarios.AnyAsync(u => u.Codigo == vm.Codigo))
                ModelState.AddModelError(nameof(vm.Codigo), "El código ya existe.");

            bool seraTecnico = vm.RolId == tecnicoid;
            // Validación si es técnico
            if (seraTecnico && vm.NivelId == null)
                ModelState.AddModelError(nameof(vm.NivelId), "El Nivel es obligatorio para técnicos.");

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(vm);
                return View(vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var usuario = new Usuario
                {
                    Usher = vm.Usher.Trim(),
                    Email = vm.Email.Trim(),
                    NombreCompleto = vm.NombreCompleto.Trim(),
                    Codigo = vm.Codigo.Trim(),
                    RolId = vm.RolId,
                    Activo = vm.Activo
                };

                var (hash, salt) = PasswordHasher.Hash(vm.Password);
                usuario.PasswordHash = hash;
                usuario.PasswordSalt = salt;

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync(); // genera Id

                if (seraTecnico)
                {
                    _context.Tecnicos.Add(new Tecnico
                    {
                        Id = usuario.Id,                 // clave compartida
                        NivelId = vm.NivelId!.Value,
                        Disponible = false
                    });
                    await _context.SaveChangesAsync();
                }

                await tx.CommitAsync();
                TempData["Ok"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", $"Error inesperado: {ex.GetBaseException().Message}");
                await CargarCombosAsync(vm);
                return View(vm);
            }
        }
            // GET: Usuarios/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["Alert"] = "El Identificador no es Válido";
                return RedirectToAction(nameof(Index));
            }
            var usuario = await _context.Usuarios
             .AsNoTracking()
             .Include(x => x.Rol)
             .FirstOrDefaultAsync(x => x.Id == id);

            if (usuario == null) return NotFound();

            var tecnico = await _context.Tecnicos
                  .AsNoTracking()
                  .Include(x => x.Nivel)
                  .FirstOrDefaultAsync(x => x.Id == id);

            var vm = new UsuarioVM
            {
                Id = usuario.Id,
                Usher = usuario.Usher,
                Email = usuario.Email,
                NombreCompleto = usuario.NombreCompleto,
                Codigo = usuario.Codigo,
                RolId = usuario.RolId,
                Activo = usuario.Activo,
                NivelId = tecnico?.NivelId
            };

            await CargarCombosAsync(vm);
            return View(vm);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioVM vm)
        {
            if (id != vm.Id)
            {
                TempData["Alert"] = "El Identificador no es válido.";
                return RedirectToAction(nameof(Index));
            }

            // Cargar usuario base (queda trackeado)
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario is null) return NotFound();

            //manejo de contraseña vacia
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.Remove(nameof(UsuarioVM.Password));

            //Validaciones de duplicados
            if (usuario.Email != vm.Email && await _context.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.Email == vm.Email && u.Id != vm.Id))
                ModelState.AddModelError(nameof(vm.Email), "El correo ya está registrado por otro usuario.");

            if (usuario.Usher != vm.Usher && await _context.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.Usher == vm.Usher && u.Id != vm.Id))
                ModelState.AddModelError(nameof(vm.Usher), "El usuario ya existe en otra cuenta.");

            if (usuario.Codigo != vm.Codigo && await _context.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.Codigo == vm.Codigo && u.Id != vm.Id))
                ModelState.AddModelError(nameof(vm.Codigo), "El código ya existe.");

            bool seraTecnico = vm.RolId == tecnicoid;

            if (seraTecnico && vm.NivelId == null)
                ModelState.AddModelError(nameof(vm.NivelId), "El Nivel es obligatorio para técnicos.");

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(vm);
                return View(vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                //Actualizar entidad base (Usuario)
                usuario.Usher = vm.Usher.Trim();
                usuario.Email = vm.Email.Trim();
                usuario.NombreCompleto = vm.NombreCompleto.Trim();
                usuario.Codigo = vm.Codigo.Trim();
                usuario.RolId = vm.RolId;
                usuario.Activo = vm.Activo;

                if (!string.IsNullOrWhiteSpace(vm.Password))
                {
                    var (hash, salt) = PasswordHasher.Hash(vm.Password);
                    usuario.PasswordHash = hash;
                    usuario.PasswordSalt = salt;
                }

                await _context.SaveChangesAsync(); //guardar cambios de la base

                //Estado actual de técnico
                bool esTecnicoHoy = await _context.Tecnicos
                    .AsNoTracking()
                    .AnyAsync(t => t.Id == vm.Id);

                if (seraTecnico && !esTecnicoHoy)
                {
                    _context.Tecnicos.Add(new Tecnico
                    {
                        Id = vm.Id,
                        NivelId = vm.NivelId!.Value,
                        Disponible = false
                    });
                    await _context.SaveChangesAsync();
                }
                else if (!seraTecnico && esTecnicoHoy)
                {
                    // Eliminar SOLO el perfil técnico (no afecta Usuario)
                    var stubDel = new Tecnico { Id = vm.Id };
                    _context.Entry(stubDel).State = EntityState.Deleted;
                    await _context.SaveChangesAsync();
                }
                else if (seraTecnico && esTecnicoHoy)
                {
                    // Uactualizar
                    var stubUpd = new Tecnico { Id = vm.Id, NivelId = vm.NivelId!.Value };
                    _context.Attach(stubUpd);
                    _context.Entry(stubUpd).Property(t => t.NivelId).IsModified = true;
                    await _context.SaveChangesAsync();
                }

                await tx.CommitAsync();
                TempData["Ok"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "El registro fue modificado por otro usuario. Recarga e inténtalo de nuevo.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", $"Error inesperado: {ex.GetBaseException().Message}");
            }

            await CargarCombosAsync(vm);
            return View(vm);
        }

        // GET: /Usuarios/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                {
                    TempData["Alert"] = "El Identificador no es Válido.";
                    return RedirectToAction(nameof(Index));
                }
            }
            var usuario= await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            var tec = await _context.Tecnicos
                .Include(u => u.Nivel)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario is null) return NotFound();

            var vm = new UsuarioVM
            {
                Id= usuario.Id,
                Usher = usuario.Usher,
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

        // POST: /Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //Validar el ID
            if (id <= 0)
            {
                TempData["Alert"] = "El Identificador no es Válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                //Buscar el usuario. 
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                    return NotFound();
                
                //Eliminar el usuario. EF Core se encarga de la herencia (Usuario/Tecnico).
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                TempData["Ok"] = "Usuario eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Manejo de errores de base de datos (ej. restricciones de clave externa)
                TempData["Error"] = $"No se pudo eliminar: {ex.GetBaseException().Message}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error inesperado: {ex.GetBaseException().Message}";
                return RedirectToAction(nameof(Index));
            }
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
            TempData["Msg"] = $"Contraseña de {usuario.Usher} restablecida correctamente.";
            return RedirectToAction(nameof(Index));
            
        }
        private async Task CargarCombosAsync(UsuarioVM vm)
        {
            vm.Roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Nombre)
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre })
                .ToListAsync();

            vm.Niveles = await _context.Niveles
                .AsNoTracking()
                .OrderBy(n => n.Nombre)
                .Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Nombre })
                .ToListAsync();
        }
    }
}
