using DGASoporte.Data;
using DGASoporte.Models;
using DGASoporte.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace DGASoporte.Controllers
{
    [Authorize(Roles = "Admin")]  
    public class UsuarioController : Controller
    {
        private readonly DGADbContext _context;

        private const int tecnicoid = 2;

        public UsuarioController(DGADbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
           
           
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u => u.Usher)
                .ToListAsync();


            return PartialView("_usuarios", usuarios);
           

        }
        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                TempData["Alert"] = "El Identificador no es Válido";
                return Json(new { success = false, message = "El Identificador no es válido." });
            }
            var vm = await _context.Usuarios
           .AsNoTracking()
           .Where(u => u.Id == id)
           .Select(u => new UsuarioVM
           {
               Id= u.Id,
               Usher = u.Usher,
               Email = u.Email,
               NombreCompleto = u.NombreCompleto,
               Codigo = u.Codigo,
               RolId = u.RolId,
               RolNombre = u.Rol != null ? u.Rol.Nombre : null,
               Activo = u.Activo,
               FechaCracion = u.CreadoEn.ToLocalTime(),
           })
            .FirstOrDefaultAsync(ct);

            if (vm is null) return NotFound();

            return PartialView("_UsuarioDetalle", vm);
            

        }

        // GET: Usuarios/Create
        public async Task<IActionResult> Create(int? rolId, CancellationToken ct)
        {
            var vm = new UsuarioVM { Activo = true };
            if (rolId != null)
            {
                vm.RolId = rolId;
            }
            await CargarCombosAsync(vm, ct);
            
            return PartialView("_UsuarioCrear", vm);
            
        }
        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioVM vm, CancellationToken ct)
        {

            // ---- VALIDACIONES DE DUPLICADOS ----
            if (await _context.Usuarios.AnyAsync(u => u.Email == vm.Email, ct))
                ModelState.AddModelError(nameof(vm.Email), "El correo ya está registrado.");

            if (await _context.Usuarios.AnyAsync(u => u.Usher == vm.Usher, ct))
                ModelState.AddModelError(nameof(vm.Usher), "El usuario ya existe.");

            if (await _context.Usuarios.AnyAsync(u => u.Codigo == vm.Codigo, ct))
                ModelState.AddModelError(nameof(vm.Codigo), "El código ya existe.");

            bool seraTecnico = vm.RolId == tecnicoid;

            if (!seraTecnico)
            {
                ModelState.Remove(nameof(UsuarioVM.NivelId));
            }

            // ---- MODELO INVÁLIDO ----
            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(vm, ct);
                return PartialView("_UsuarioCrear", vm);              
            }

            // ---- GUARDADO ----
            using var tx = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var usuario = new Usuario
                {
                    Usher = vm.Usher.Trim(),
                    Email = vm.Email.Trim(),
                    NombreCompleto = vm.NombreCompleto.Trim(),
                    Codigo = vm.Codigo.Trim(),
                    RolId = vm.RolId!.Value,
                    Activo = vm.Activo
                };

                var (hash, salt) = PasswordHasher.Hash(vm.Password);
                usuario.PasswordHash = hash;
                usuario.PasswordSalt = salt;

                _context.Add(usuario);
                await _context.SaveChangesAsync(ct);  

                if (seraTecnico)
                {
                    _context.Tecnicos.Add(new Tecnico
                    {
                        Id = usuario.Id,
                        NivelId = vm.NivelId!.Value,
                        Disponible = false
                    });

                    await _context.SaveChangesAsync(ct);
                }

                await tx.CommitAsync(ct);
                TempData["Ok"] = "Usuario creado correctamente.";

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);

                ModelState.AddModelError(string.Empty,
                    $"Error inesperado: {ex.GetBaseException().Message}");

                await CargarCombosAsync(vm, ct);
                return PartialView("_UsuarioCrear", vm);

            }
        }

        // GET: Usuarios/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                TempData["Alert"] = "El Identificador no es Válido";
                return Json(new { success = false, message = "El Identificador no es válido." });
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

            await CargarCombosAsync(vm, ct);
            return PartialView("_UsuarioEditar", vm);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioVM vm, CancellationToken ct)
        {
            if (id != vm.Id)
            {
                TempData["Alert"] = "El Identificador no es válido.";
                return Json(new { success = false, message = "El Identificador no es válido." });
            }

            // Cargar usuario base (queda trackeado)
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id, ct);
            if (usuario is null) return NotFound();

            // Manejo de contraseña vacía
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.Remove(nameof(UsuarioVM.Password));

            // Validaciones de duplicados
            if (usuario.Email != vm.Email && await _context.Usuarios
                    .AsNoTracking()
                    .AnyAsync(u => u.Email == vm.Email && u.Id != vm.Id, ct))
                ModelState.AddModelError(nameof(vm.Email), "El correo ya está registrado por otro usuario.");

            if (usuario.Usher != vm.Usher && await _context.Usuarios
                    .AsNoTracking()
                    .AnyAsync(u => u.Usher == vm.Usher && u.Id != vm.Id, ct))
                ModelState.AddModelError(nameof(vm.Usher), "El usuario ya existe en otra cuenta.");

            if (usuario.Codigo != vm.Codigo && await _context.Usuarios
                    .AsNoTracking()
                    .AnyAsync(u => u.Codigo == vm.Codigo && u.Id != vm.Id, ct))
                ModelState.AddModelError(nameof(vm.Codigo), "El código ya existe.");

            bool seraTecnico = vm.RolId == tecnicoid;

            if (!seraTecnico)
                ModelState.Remove(nameof(UsuarioVM.NivelId));

            // ---- Modelo inválido ----
            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(vm, ct);
                return PartialView("_UsuarioEditar", vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                // Actualizar entidad base (Usuario)
                usuario.Usher = vm.Usher.Trim();
                usuario.Email = vm.Email.Trim();
                usuario.NombreCompleto = vm.NombreCompleto.Trim();
                usuario.Codigo = vm.Codigo.Trim();
                usuario.RolId = vm.RolId!.Value;
                usuario.Activo = vm.Activo;

                if (!string.IsNullOrWhiteSpace(vm.Password))
                {
                    var (hash, salt) = PasswordHasher.Hash(vm.Password);
                    usuario.PasswordHash = hash;
                    usuario.PasswordSalt = salt;
                }

                await _context.SaveChangesAsync(ct); 

                // Estado actual de técnico
                bool esTecnicoHoy = await _context.Tecnicos
                    .AsNoTracking()
                    .AnyAsync(t => t.Id == vm.Id, ct);

                if (seraTecnico && !esTecnicoHoy)
                {
                    _context.Tecnicos.Add(new Tecnico
                    {
                        Id = vm.Id,
                        NivelId = vm.NivelId!.Value,
                        Disponible = false
                    });
                    await _context.SaveChangesAsync(ct);
                }
                else if (!seraTecnico && esTecnicoHoy)
                {
                    // Eliminar SOLO el perfil técnico
                    var stubDel = new Tecnico { Id = vm.Id };
                    _context.Entry(stubDel).State = EntityState.Deleted;
                    await _context.SaveChangesAsync(ct);
                }
                else if (seraTecnico && esTecnicoHoy)
                {
                    // Actualizar nivel
                    var stubUpd = new Tecnico { Id = vm.Id, NivelId = vm.NivelId!.Value };
                    _context.Attach(stubUpd);
                    _context.Entry(stubUpd).Property(t => t.NivelId).IsModified = true;
                    await _context.SaveChangesAsync(ct);
                }

                await tx.CommitAsync(ct);
                TempData["Ok"] = "Usuario actualizado correctamente.";

                 return Json(new { success = true });
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync(ct);
                ModelState.AddModelError("", "El registro fue modificado por otro usuario. Recarga e inténtalo de nuevo.");
                await CargarCombosAsync(vm, ct);

                 return PartialView("_UsuarioEditar", vm);

            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                ModelState.AddModelError("", $"Error inesperado: {ex.GetBaseException().Message}");
                await CargarCombosAsync(vm, ct);

                return PartialView("_UsuarioEditar", vm);

            }

        }

        // GET: /Usuarios/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            if (id <= 0)
            {            
                    TempData["Alert"] = "El Identificador no es Válido.";
                return Json(new { success = false, message = "El Identificador no es válido." });
            }
            var tec = await _context.Tecnicos
                .Include(u => u.Nivel)
                .FirstOrDefaultAsync(u => u.Id == id);

            var vm = await _context.Usuarios
              .AsNoTracking()
              .Where(u => u.Id == id)
              .Select(u => new UsuarioVM
              {
                  Id = u.Id,
                  Usher = u.Usher,
                  Email = u.Email,
                  NombreCompleto = u.NombreCompleto,
                  Codigo = u.Codigo,
                  RolId = u.RolId,
                  RolNombre = u.Rol != null ? u.Rol.Nombre : null,
                  Activo = u.Activo,
                  FechaCracion = u.CreadoEn.ToLocalTime(),
                  NivelNombre = tec != null && tec.Nivel != null ? tec.Nivel.Nombre : null
              })
               .FirstOrDefaultAsync(ct);

            if (vm is null) return NotFound();

            return PartialView("_UsuarioEliminar", vm);

        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            // Validar el ID
            if (id <= 0)
            {
                TempData["Alert"] = "El Identificador no es Válido.";
                return Json(new { success = false, message = "El Identificador no es Válido." });
            }

            try
            {
                // Buscar el usuario
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == id, ct);

                if (usuario == null)
                {
                     return Json(new { success = false, message = "Usuario no encontrado." });
                }

                // Eliminar el usuario. EF Core se encarga de la herencia (Usuario/Tecnico).
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync(ct);

                TempData["Ok"] = "Usuario eliminado correctamente";
                return Json(new { success = true, message = "Usuario eliminado correctamente" });

            }
            catch (DbUpdateException ex)
            {
                var errMsg = $"No se pudo eliminar: {ex.GetBaseException().Message}";

                TempData["Error"] = errMsg;
                return Json(new { success = false, message = errMsg });

            }
            catch (Exception ex)
            {
                var errMsg = $"Error inesperado: {ex.GetBaseException().Message}";

                TempData["Error"] = errMsg;
                return Json(new { success = false, message = errMsg });

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
                usuario.finBloqueo = hasta ?? DateTime.Now.ToLocalTime().AddYears(100);
            }

            usuario.ActualizadoEn = DateTime.Now.ToLocalTime();
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Usuario bloqueado correctamente.";
            return Json(new { success = true });
        }

        private async Task CargarCombosAsync(UsuarioVM vm, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            vm.Roles = await _context.Roles
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem(c.Nombre, c.Id.ToString()))
                .ToListAsync(ct);

            ct.ThrowIfCancellationRequested();

            vm.Niveles = await _context.Niveles
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem(c.Nombre, c.Id.ToString()))
                .ToListAsync(ct);
        }
    }
}
