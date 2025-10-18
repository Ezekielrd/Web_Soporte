using DGASoporte.Data;
using DGASoporte.Models;
using DGASoporte.Seguridad;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;



namespace DGASoporte.Controllers
{
    public class CuentaController : Controller
    {
        public readonly DGADbContext _context;

        public CuentaController(DGADbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
           return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm)
        {
           
                return Redirect(vm.ReturnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Denied() => View();
    }
}
