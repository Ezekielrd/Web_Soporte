using System.Diagnostics;
using DGASoporte.Data;
using DGASoporte.Models;
using Microsoft.AspNetCore.Mvc;

namespace DGASoporte.Controllers
{
    public class HomeController : Controller
    {
        private readonly DGADbContext _context;

        public HomeController(DGADbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> TestConexion()
        {
            bool canConnect = await _context.Database.CanConnectAsync();

            if (canConnect)
            {
                return Content("Conexión a la base de datos exitosa.");
            }
            else
            {
                return Content("No se pudo conectar a la base de datos.");
            }
        }
    }
}
