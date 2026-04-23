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
        private readonly LoginRateLimitService _loginRateLimitService;

        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            AuditService auditService,
            LoginRateLimitService loginRateLimitService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditService = auditService;
            _loginRateLimitService = loginRateLimitService;
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
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP no disponible";
            var route = HttpContext.Request.Path.ToString();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // La clave de bloqueo usará correo + IP para hacer el control más estricto
            var rateLimitKey = $"{model.Email}|{ipAddress}";

            // Verificar si ya está bloqueado
            if (_loginRateLimitService.IsBlocked(rateLimitKey, out var blockedUntil))
            {
                await _auditService.LogAsync(
                    "LOGIN_BLOCKED",
                    $"Login web bloqueado temporalmente para {model.Email} desde IP {ipAddress} hasta {blockedUntil:yyyy-MM-dd HH:mm:ss} UTC",
                    null,
                    model.Email,
                    ipAddress,
                    route);

                ModelState.AddModelError(string.Empty, "Acceso temporalmente bloqueado por múltiples intentos fallidos. Intenta nuevamente más tarde.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                var failureResult = _loginRateLimitService.RegisterFailure(rateLimitKey);

                await _auditService.LogAsync(
                    failureResult.blockedNow ? "LOGIN_BLOCKED" : "LOGIN_FAILED",
                    failureResult.blockedNow
                        ? $"Se bloqueó el login web para {model.Email} desde IP {ipAddress} por múltiples intentos fallidos."
                        : $"Intento de login web fallido con correo no encontrado: {model.Email}",
                    null,
                    model.Email,
                    ipAddress,
                    route);

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
                // Reiniciar contador de fallos al iniciar correctamente
                _loginRateLimitService.Reset(rateLimitKey);

                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                await _auditService.LogAsync(
                    "LOGIN_SUCCESS",
                    $"Inicio de sesión exitoso del usuario {user.UserName}",
                    user.Id,
                    user.UserName,
                    ipAddress,
                    route);

                return RedirectToAction("Index", "Home");
            }

            var failedAttempt = _loginRateLimitService.RegisterFailure(rateLimitKey);

            await _auditService.LogAsync(
                failedAttempt.blockedNow ? "LOGIN_BLOCKED" : "LOGIN_FAILED",
                failedAttempt.blockedNow
                    ? $"Se bloqueó el login web para el usuario {user.UserName} desde IP {ipAddress} por múltiples intentos fallidos."
                    : $"Intento de login web fallido para el usuario {user.UserName}",
                user.Id,
                user.UserName,
                ipAddress,
                route);

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