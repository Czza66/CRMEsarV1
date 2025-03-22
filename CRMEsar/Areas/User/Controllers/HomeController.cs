using System.Diagnostics;
using System.Security.Claims;
using System.Security.Policy;
using System.Text.Encodings.Web;
using CRMEsar.Models;
using CRMEsar.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CRMEsar.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public readonly UrlEncoder _urlEncoder;


        public HomeController(ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            UrlEncoder urlEncoder)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
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
                var user = new ApplicationUser
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

        // Fundionalidad Login Iniciar Sesion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Acceso(AccesoVM Login)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Login.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "El usuario no existe.");
                    return View(Login);
                }

                var resultado = await _signInManager.PasswordSignInAsync(user, Login.Password, Login.RememberMe, lockoutOnFailure: true);

                if (resultado.Succeeded)
                {
                    // Validar si el usuario tiene activo el 2FA
                    var tiene2FA = await _userManager.GetTwoFactorEnabledAsync(user);
                    if (!tiene2FA)
                    {
                        return RedirectToAction("ActivarAutenticador", "Home", new { area = "User" });
                    }
                }

                if (resultado.RequiresTwoFactor)
                {
                    return RedirectToAction("VerificarCodigoAutenticador", "Home", new { area = "User"});
                }

                if (resultado.IsLockedOut)
                {
                    return View("Bloqueado");
                }

                ModelState.AddModelError(string.Empty, "Acceso inválido");
            }

            return View(Login);
        }

        //Autenticacion en dos factores
        [HttpGet]
        public async Task<IActionResult> ActivarAutenticador()
        {
            string formatoUrlAutenticador = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
            var usuario = await _userManager.GetUserAsync(User);
            await _userManager.ResetAuthenticatorKeyAsync(usuario);
            var token = await _userManager.GetAuthenticatorKeyAsync(usuario);
            //Habilitar CodigoQR
            string urlAutenticador = string.Format(formatoUrlAutenticador, _urlEncoder.Encode("CRMEsar"), _urlEncoder.Encode(usuario.Email), token);

            var ADFModel = new AutenticacionDosFactoresVM() { Token = token, urlCodigoQR = urlAutenticador };
            return View(ADFModel);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarAutenticador(AutenticacionDosFactoresVM adf)
        {
            if (!ModelState.IsValid)
            {
                return View(adf);
            }

            var usuario = await _userManager.GetUserAsync(User);
            var succeeded = await _userManager.VerifyTwoFactorTokenAsync(usuario, _userManager.Options.Tokens.AuthenticatorTokenProvider, adf.code);

            if (succeeded)
            {
                await _userManager.SetTwoFactorEnabledAsync(usuario, true);
                return RedirectToAction(nameof(ConfirmacionAutenticador));
            }

            ModelState.AddModelError("Verificar", "Su autenticación de dos factores no ha sido validada");
            adf.Token = await _userManager.GetAuthenticatorKeyAsync(usuario); // Volver a cargar token para reintento
            return View(adf);
        }

        [HttpGet]
        public IActionResult ConfirmacionAutenticador()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AccesoExternoCallBack(string error = null)
        {
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, $"Error en el acceso externo {error}");
                return View(nameof(Index));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Index));
            }
            //Acceder con el usuario en el proveedor externo
            var resultado = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (resultado.Succeeded)
            {
                //Actualizar los tokens de acceso
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                return RedirectToAction("ActivarAutenticador", "Home", new { area = "User" });
            }
            //Validacion para autenticacion en dos factores
            if (resultado.RequiresTwoFactor)
            {
                return RedirectToAction("VerificarCodigoAutenticador");
            }
            else
            {
                //Si el usuario no tiene cuenta pregunta si quiere crear una
                ViewData["NombreMostrarProveedor"] = info.ProviderDisplayName;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var nombre = info.Principal.FindFirstValue(ClaimTypes.Name);
                return View("ConfirmacionAccesoExterno");
            }

        }

        [HttpGet]
        public async Task<IActionResult> VerificarCodigoAutenticador(bool recordarDatos)
        {
            var usuario = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (usuario == null)
            {
                return View("Error");
            }
            return View(new VerificarAutenticadorVM { RecordarDatos = recordarDatos });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> VerificarCodigoAutenticador(VerificarAutenticadorVM vaVM)
        {
            vaVM.returnURL = vaVM.returnURL ?? Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View(vaVM);
            }
            var resultado = await _signInManager.TwoFactorAuthenticatorSignInAsync(vaVM.code, vaVM.RecordarDatos, rememberClient: false);
            if (resultado.Succeeded)
            {
                return LocalRedirect(vaVM.returnURL);
            }
            if (resultado.IsLockedOut)
            {
                return View("Bloqueado");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Codigo Invalido");
                return View(vaVM);
            }
        }
    }
}