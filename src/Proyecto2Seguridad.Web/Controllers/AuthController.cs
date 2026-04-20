using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto2Seguridad.Web.Models;
using Proyecto2Seguridad.Web.ViewModels;

namespace Proyecto2Seguridad.Web.Controllers
{
    
    /// Controlador responsable de autenticación: login, logout y acceso denegado.
    
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        
        /// Constructor del controlador de autenticación.
        
        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        
        /// Muestra la pantalla de inicio de sesión.
        
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        
        /// Procesa el formulario de inicio de sesión.
        
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Si el modelo no es válido, se devuelve la vista con errores.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Buscar usuario por correo.
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(model);
            }

            // Intentar iniciar sesión con username y contraseña.
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            // Si el login es exitoso, actualizar la fecha del último acceso.
            if (result.Succeeded)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
            return View(model);
        }

        
        /// Cierra la sesión del usuario autenticado.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }

        
        /// Muestra una vista amigable cuando el usuario no tiene permisos.
        
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}