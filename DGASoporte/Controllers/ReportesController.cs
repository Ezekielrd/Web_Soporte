using Microsoft.AspNetCore.Mvc;

namespace DGASoporte.Controllers
{
    public class ReportesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
