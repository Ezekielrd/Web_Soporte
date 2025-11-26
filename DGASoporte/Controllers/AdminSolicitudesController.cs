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
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Usuario)
                .Include(s => s.Unidad)
                .Include(s => s.TipoIncidencia)
                .OrderBy(s => s.Estado)
                .ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_solicitudes", solicitudes);

            return View(solicitudes);
        }
        [HttpGet]
        public async Task<IActionResult> Rechazar(int id)
        {
            var sol = await _context.Solicitudes.FindAsync(id); // ajusta el DbSet
            if (sol == null) return NotFound();

            ViewBag.SolicitudId = sol.Id;
            // Se devuelve solo el contenido del formulario (para el appModal)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_MotivoRechazo");

            return View("_MotivoRechazo"); // por si alguien entra directo
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, string motivoRechazo)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            // ✅ Validación del motivo (obligatorio)
            if (string.IsNullOrWhiteSpace(motivoRechazo) || motivoRechazo.Trim().Length < 10)
            {
                var msgMotivo = "Debe indicar un motivo de rechazo con al menos 10 caracteres.";

                if (isAjax)
                {
                    return Json(new
                    {
                        success = false,
                        message = msgMotivo,
                        // si quieres, puedes mandar redirect o dejarlo así para que el JS solo muestre el mensaje
                        redirectUrl = (string?)null
                    });
                }

                TempData["Error"] = msgMotivo;
                return RedirectToAction(nameof(Index));
            }

            var solicitud = await _context.Solicitudes
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
            {
                var msgNotFound = "La solicitud no existe.";
                if (isAjax)
                {
                    return Json(new
                    {
                        success = false,
                        message = msgNotFound,
                        redirectUrl = Url.Action("Index", "AdminSolicitudes")
                    });
                }

                TempData["Error"] = msgNotFound;
                return RedirectToAction(nameof(Index));
            }

            if (solicitud.Estado != EstadoS.Enviada)
            {
                var msg = "La solicitud ya fue evaluada.";

                if (isAjax)
                {
                    return Json(new
                    {
                        success = false,
                        message = msg,
                        redirectUrl = Url.Action("Index", "AdminSolicitudes")
                    });
                }

                TempData["Error"] = msg;
                return RedirectToAction(nameof(Index));
            }

            solicitud.Estado = EstadoS.Rechazada;
            solicitud.MotivoRechazo = motivoRechazo;

            _context.Solicitudes.Update(solicitud);
            await _context.SaveChangesAsync();

            var okMsg = "Solicitud rechazada.";
            TempData["Mensaje"] = okMsg;

            if (isAjax)
            {
                return Json(new
                {
                    success = true,
                    message = okMsg,
                    redirectUrl = Url.Action("Index", "AdminSolicitudes")
                });
            }

            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            var solicitud = await _context.Solicitudes
            .Include(s => s.Usuario)
            .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound();

            if (solicitud.Estado != EstadoS.Enviada)
            {
                TempData["Error"] = "La solicitud ya fue evaluada.";
                if (isAjax)
                {
                    // ⚠️ desde el modal: solo devolvemos el card
                    return PartialView("_solicitudes");
                }
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Details(int id)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Usuario)
                .Include(s => s.Unidad)
                .Include(s => s.TipoIncidencia)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null) return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_SolicitudDetalle", solicitud);

            return View(solicitud);
        }
    }
}
