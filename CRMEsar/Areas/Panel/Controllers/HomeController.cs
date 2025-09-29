using CRMEsar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CRMEsar.Areas.Panel.Controllers
{
    [Area("Panel")]
    [Authorize(Roles = "Admin,Prestador")]
    public class HomeController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        public HomeController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> cerrarSesion() 
        {
            await _signInManager.SignOutAsync(); //Eliminar Cookie de applicacion
            TempData["CerrarSesion"] = "Que vuelvas pronto!";
            return RedirectToAction("Index", "Home", new { area = "User" });
        }

    }
}
