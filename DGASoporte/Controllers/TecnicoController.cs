using DGASoporte.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DGASoporte.Controllers
{
    public class TecnicoController : Controller
    {
        public readonly DGADbContext _context;

        public TecnicoController(DGADbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            // Suponiendo que el técnico autenticado tiene su Id en Claims
           // int tecnicoId = int.Parse(User.FindFirst("TecnicoId").Value);

            var tareas = await _context.Tareas
                .Where(t => t.TecnicoId == 5)
                .OrderByDescending(t => t.FechaAsignacion)
                .ToListAsync();

            return View(tareas);
        }
    }
}
