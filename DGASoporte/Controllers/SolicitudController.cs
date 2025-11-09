using DGASoporte.Data;
using DGASoporte.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DGASoporte.Controllers
{
    public class SolicitudController : Controller
    {
        private readonly DGADbContext _context;
        private readonly ILogger<SolicitudController> _logger;

        public SolicitudController(DGADbContext context, ILogger<SolicitudController> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        // GET: Solicitud/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new SolicitudVM
            {
                Unidades = await _context.Unidades
                .OrderBy(u => u.Nombre)
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Nombre })
                .ToListAsync()
            };

            return View(vm);
        }

        // POST: SolicitudIncidencia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SolicitudVM vm)
        {
          
            if (!ModelState.IsValid)
            {
                await CargarUnidadesAsync(vm);
                return View(vm);
            }

            // Mapear
            var entidad = new Solicitud
            {
                Titulo = vm.Titulo.Trim(),
                Descripcion = vm.Descripcion.Trim(),
                FechaCreacion = DateTime.Now,
                UnidadId = vm.UnidadId,
                Tipo = vm.Tipo!.Value,
                UsuarioId=2
            };

            _context.Solicitudes.Add(entidad);
            await _context.SaveChangesAsync();

            TempData["ok"] = "Solicitud registrada con éxito.";
            return RedirectToAction(nameof(Create));
        }

        private async Task CargarUnidadesAsync(SolicitudVM vm)
        {
            vm.Unidades = await _context.Unidades
                .OrderBy(u => u.Nombre)
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Nombre })
                .ToListAsync();
        }      
    }
}