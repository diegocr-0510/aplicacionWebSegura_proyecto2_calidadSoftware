using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto2Seguridad.Web.Models;
using Proyecto2Seguridad.Web.Services;
using Proyecto2Seguridad.Web.ViewModels;

namespace Proyecto2Seguridad.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditService _auditService;

        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            AuditService auditService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditService = auditService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Obtener IP y ruta actual
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP no disponible";
            var route = HttpContext.Request.Path;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                // Registrar intento fallido con correo no encontrado
                await _auditService.LogAsync(
                    "LOGIN_FAILED",
                    $"Intento de login fallido con correo no encontrado: {model.Email}",
                    null,
                    model.Email,
                    ipAddress,
                    route!);

                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Actualizar último login
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Registrar login exitoso
                await _auditService.LogAsync(
                    "LOGIN_SUCCESS",
                    $"Inicio de sesión exitoso del usuario {user.UserName}",
                    user.Id,
                    user.UserName,
                    ipAddress,
                    route!);

                return RedirectToAction("Index", "Home");
            }

            // Registrar login fallido por contraseña incorrecta
            await _auditService.LogAsync(
                "LOGIN_FAILED",
                $"Intento de login fallido para el usuario {user.UserName}",
                user.Id,
                user.UserName,
                ipAddress,
                route!);

            ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}