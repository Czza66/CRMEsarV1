using CRMEsar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CRMEsar.Areas.Panel.Controllers
{
    [Area("Panel")]
    [Authorize(Roles = "Admin")]
    public class InfoUserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public InfoUserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var InfoUser = await _userManager.GetUserAsync(User);
            return View(InfoUser);
        }
    }
}
