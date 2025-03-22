using System.Diagnostics;
using CRMEsar.Models;
using CRMEsar.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CRMEsar.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, 
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegisterVM rVM)
        {
            if (ModelState.IsValid)
            {
                var user = new AspNetUser
                {
                    UserName = rVM.Email,
                    Email = rVM.Email,
                    NombreCompleto = $"{rVM.Nombre1} {rVM.Nombre2 ?? ""} {rVM.Apellido1} {rVM.Apellido2 ?? ""}".Trim(),
                    NumeroDocumento = rVM.NumeroDocumento,
                    FechaRegistro = DateTime.Now,
                    Nombre1 = rVM.Nombre1,
                    Nombre2 = rVM.Nombre2,
                    Apellido2 = rVM.Apellido2,
                    Apellido1 = rVM.Apellido1
                };
                var resultado = await _userManager.CreateAsync(user, rVM.Password);
                if (resultado.Succeeded)
                {
                    return RedirectToAction("Index", new { correcto = true });
                }
                else 
                {
                    return RedirectToAction("Index", new { incorrecto = true });
                }
            }
            return View(rVM);
        }

    }
}
