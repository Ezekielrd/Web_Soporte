using DGASoporte.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DGASoporte.Infraestructura;

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
            int tecnicoId = User.GetRequiredUserId();

            var tareas = await _context.Tareas
                .Where(t => t.TecnicoId == tecnicoId)
                .OrderByDescending(t => t.Prioridad)
                .ToListAsync();

            return View(tareas);
        }
    }
}
