using System.Diagnostics;
using System.Security.Claims;
using System.Security.Policy;
using System.Text.Encodings.Web;
using CRMEsar.AccesoDatos.Data.Repository.IRepository;
using CRMEsar.AccesoDatos.Services.Logs;
using CRMEsar.AccesoDatos.Services.Notificaciones;
using CRMEsar.Models;
using CRMEsar.Models.ViewModels.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing.Template;

namespace CRMEsar.Areas.User.Controllers
{
    [Area("User")]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public readonly UrlEncoder _urlEncoder;
        public readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly ILogService _logService;
        private readonly INotificacionesService _notificaciones;

        private const string NombreEntidadNormalizado = "ASPNETUSERS";

        public HomeController(ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            UrlEncoder urlEncoder,
            IContenedorTrabajo contenedorTrabajo,
            ILogService logService,
            INotificacionesService notificaciones)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
            _contenedorTrabajo = contenedorTrabajo;
            _logService = logService;
            _notificaciones = notificaciones;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegisterVM rVM)
        {
            if (!ModelState.IsValid)
            {
                return View(rVM);
            }

            // Buscar estado "Activo" asociado a esa entidad
            var estadoActivo = _contenedorTrabajo.Estado.GetFirstOrDefault(
                e => e.Nombre == "Activo" &&
                     e.Entidad.NormalizedName == NombreEntidadNormalizado,
                includeproperties: "Entidad"
            );

            if (estadoActivo == null)
            {
                ModelState.AddModelError(string.Empty, "No se encontró el estado activo para esta entidad.");
                return View(rVM);
            }

            // Crear el usuario
            var user = new ApplicationUser
            {
                UserName = rVM.Email,
                Email = rVM.Email,
                NombreCompleto = $"{rVM.Nombre1} {rVM.Nombre2 ?? ""} {rVM.Apellido1} {rVM.Apellido2 ?? ""}".Trim(),
                NumeroDocumento = rVM.NumeroDocumento,
                FechaRegistro = DateTime.Now,
                Nombre1 = rVM.Nombre1,
                Nombre2 = rVM.Nombre2,
                Apellido1 = rVM.Apellido1,
                Apellido2 = rVM.Apellido2,
                EstadoId = estadoActivo.EstadoId
            };

            var resultado = await _userManager.CreateAsync(user, rVM.Password);
            if (resultado.Succeeded)
            {
                await _notificaciones.CrearNotificacionAsync(
                    userId: user.Id.ToString(),
                    titulo:"Bienvenida/o a nuestro CRM",
                    mensaje:"Por favor verifica tu correo electronico para evitar bloquear tu cuenta",
                    nombreTabla:"ApplicationUser",
                    tipoNotificacionNormalizedName: "SUCCES" //MISTAKE, WARNING
                    );

                await _userManager.AddToRoleAsync(user, "Admin");
                return RedirectToAction("Index", new { correcto = true });
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
                    TempData["ErrorLogin"] = "Usuario o contraseña inválidos.";
                    return RedirectToAction(nameof(Index));
                }

                // Validar que el usuario esté ACTIVO
                var estadoActivo = _contenedorTrabajo.Estado.GetFirstOrDefault(
                    e => e.Nombre == "Activo" &&
                         e.Entidad.NormalizedName == NombreEntidadNormalizado,
                    includeproperties: "Entidad"
                );
                if (estadoActivo == null || user.EstadoId != estadoActivo.EstadoId)
                {
                    TempData["ErrorLogin"] = "Tienes problemas con tu cuenta? contacta al administrador.";
                    return RedirectToAction(nameof(Index));
                }
                // Login normal
                var resultado = await _signInManager.PasswordSignInAsync(user, Login.Password, Login.RememberMe, lockoutOnFailure: true);
                if (resultado.Succeeded)
                {
                    var tiene2FA = await _userManager.GetTwoFactorEnabledAsync(user);
                    if (!tiene2FA)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: Login.RememberMe);
                        return RedirectToAction("ActivarAutenticador", "Home", new { area = "User" });
                    }
                }
                if (resultado.RequiresTwoFactor)
                {
                    return RedirectToAction("VerificarCodigoAutenticador", "Home", new { area = "User" });
                }
                if (resultado.IsLockedOut)
                {
                    return View("Bloqueado");
                }
                await _logService.RegistrarAsync<ApplicationUser>(
                    user.Id.ToString(),
                    "INICIOSESION",
                    user,
                    false,
                    "Inicio de sesion incorrecto por contraseña o correo"
                    );
                TempData["ErrorLogin"] = "Usuario o contraseña inválidos.";
                return RedirectToAction(nameof(Index));
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
            string urlAutenticador = string.Format(formatoUrlAutenticador, _urlEncoder.Encode("Fundacion Esar - CRM"), _urlEncoder.Encode(usuario.Email), token);

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

                await _signInManager.SignOutAsync();

                await _signInManager.SignInAsync(usuario, isPersistent: false);

                await _logService.RegistrarAsync<ApplicationUser>(
                    usuario.Id.ToString(),
                    "2FA",
                    usuario,
                    true,
                    "El usuario configuro el 2FA de microsoft authenticator"
                    );

                TempData["VerificacionCorrecta"] = "Intenta iniciar sesion de nuevo";
                return RedirectToAction("Index", "Home", new { area = "User" });
            }
            TempData["ErrorAuth"] = "El Codigo no coincide, intentalo de nuevo";
            adf.Token = await _userManager.GetAuthenticatorKeyAsync(usuario);
            string urlAutenticador = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                _urlEncoder.Encode("Fundacion Esar - CRM"),
                _urlEncoder.Encode(usuario.Email),
                adf.Token
            );
            adf.urlCodigoQR = urlAutenticador;
            return View(adf);
        }

        [HttpGet]
        public IActionResult ConfirmacionAutenticador()
        {
            return View();
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
                var usuario = await _signInManager.GetTwoFactorAuthenticationUserAsync();

                // Esta es la clave para que se generen los claims correctamente
                var principal = await _signInManager.CreateUserPrincipalAsync(usuario);
                await _signInManager.SignOutAsync(); // Salir de cualquier sesión anterior
                await _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, principal);

                // Confirmamos si ahora los claims están
                var claims = principal.Claims.ToList();
                await _logService.RegistrarAsync<ApplicationUser>(
                    usuario.Id.ToString(),
                    "INICIOSESION",
                    usuario,
                    true,
                    "Inicio de sesion correcto"
                    );

                return RedirectToAction("Index", "Home", new { area = "Panel" });
            }

            if (resultado.IsLockedOut)
            {
                return View("Bloqueado");
            }

            ModelState.AddModelError(string.Empty, "Codigo Invalido");
            return View(vaVM);
        }
    }
}