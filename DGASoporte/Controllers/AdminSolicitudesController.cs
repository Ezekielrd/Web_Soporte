using DGASoporte.Data;
using DGASoporte.Migrations;
using DGASoporte.Models;
using DGASoporte.Models.Enumeradores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;


namespace DGASoporte.Controllers
{
    public class AdminSolicitudesController : Controller
    {
        private readonly DGADbContext _context;
        public AdminSolicitudesController(DGADbContext ctx) => _context = ctx;

        // Pendientes (Enviadas)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pendientes = await _context.Solicitudes
                .Include(s => s.Usuario)
                .Include(s => s.TipoIncidencia)
                .Where(s => s.Estado == EstadoS.Enviada)
                .OrderBy(s => s.FechaCreacion)
                .ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_tareas", pendientes); // solo el contenido

            return View(pendientes);
        }
        [HttpGet]
        // GET: ver detalle para evaluar
        public async Task<IActionResult> Evaluar(int id)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound();

            if (solicitud.Estado != EstadoS.Enviada)
            {
                TempData["Error"] = "La solicitud ya fue evaluada.";
                return RedirectToAction(nameof(Index));
            }

            return View(solicitud);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, string motivoRechazo)
        {
            var solicitud = await _context.Solicitudes
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound();

            if (solicitud.Estado != EstadoS.Enviada)
            {
                TempData["Error"] = "La solicitud ya fue evaluada.";
                return RedirectToAction(nameof(Index));
            }

            solicitud.Estado = EstadoS.Rechazada;
            solicitud.MotivoRechazo = motivoRechazo;

            _context.Solicitudes.Update(solicitud);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Solicitud rechazada.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var solicitud = await _context.Solicitudes
            .Include(s => s.Usuario)
            .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound();

            if (solicitud.Estado != EstadoS.Enviada)
            {
                TempData["Error"] = "La solicitud ya fue evaluada.";
                return RedirectToAction("Pendientes", "SolicitudesAdmin");
            }

            var vm = new TareaFormVM
            {
                Titulo = solicitud.Titulo,
                Descripcion = solicitud.Descripcion,
                Tecnicos = await _context.Tecnicos
                    .Include(t => t.Usuario)   // Para acceder a Usuario
                    .OrderBy(t => t.Usuario.NombreCompleto)
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Usuario.NombreCompleto
                    })
                    .ToListAsync(),
            };
            solicitud.Estado = EstadoS.Aprobada;
            _context.Solicitudes.Update(solicitud);
            await _context.SaveChangesAsync();


            // Redirigimos al flujo de creación de tareas
            return View("~/Views/Tarea/Create.cshtml", vm);
        }
    }
}
