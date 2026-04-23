using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto2Seguridad.Web.Models;
using Proyecto2Seguridad.Web.Services;
using Proyecto2Seguridad.Web.ViewModels;

namespace Proyecto2Seguridad.Web.Controllers
{
    // Solo el SuperAdmin puede administrar usuarios
    [Authorize(Roles = "SuperAdmin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly AuditService _auditService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            AuditService auditService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditService = auditService;
        }

        // Muestra el listado de usuarios
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<UserFormViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var currentRole = roles.FirstOrDefault() ?? "Sin rol";

                userList.Add(new UserFormViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Role = currentRole,
                    LastLoginAt = user.LastLoginAt
                });
            }

            return View(userList);
        }

        // Muestra el formulario para crear usuario
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r != null)
                .ToList();

            return View();
        }

        // Guarda un nuevo usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            ViewBag.Roles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r != null)
                .ToList();

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "La contraseña es obligatoria.");
            }

            var existingUserByUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByUserName != null)
            {
                ModelState.AddModelError("UserName", "Ya existe un usuario con ese nombre.");
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Email", "Ya existe un usuario con ese correo.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password!);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            // Registrar auditoría de creación de usuario
            await RegisterAuditAsync(
                "USER_CREATE",
                $"Se creó el usuario {user.UserName} con rol {model.Role}");

            return RedirectToAction(nameof(Index));
        }

        // Muestra el formulario de edición
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault() ?? string.Empty;

            ViewBag.Roles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r != null)
                .ToList();

            var model = new UserFormViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = currentRole,
                LastLoginAt = user.LastLoginAt
            };

            return View(model);
        }

        // Guarda cambios de un usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            ViewBag.Roles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r != null)
                .ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            var existingUserByUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByUserName != null && existingUserByUserName.Id != user.Id)
            {
                ModelState.AddModelError("UserName", "Ya existe otro usuario con ese nombre.");
                return View(model);
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
            {
                ModelState.AddModelError("Email", "Ya existe otro usuario con ese correo.");
                return View(model);
            }

            user.UserName = model.UserName;
            user.Email = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View(model);
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var previousRole = currentRoles.FirstOrDefault() ?? "Sin rol";

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            // Registrar auditoría de edición de usuario
            await RegisterAuditAsync(
                "USER_EDIT",
                $"Se editó el usuario {user.UserName}. Rol anterior: {previousRole}. Rol nuevo: {model.Role}");

            return RedirectToAction(nameof(Index));
        }

        // Muestra confirmación de eliminación
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault() ?? "Sin rol";

            var model = new UserFormViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = currentRole,
                LastLoginAt = user.LastLoginAt
            };

            return View(model);
        }

        // Elimina un usuario
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "No puedes eliminar tu propio usuario mientras estás autenticado.";
                return RedirectToAction(nameof(Index));
            }

            var deletedUserName = user.UserName ?? "Usuario sin nombre";

            await _userManager.DeleteAsync(user);

            // Registrar auditoría de eliminación de usuario
            await RegisterAuditAsync(
                "USER_DELETE",
                $"Se eliminó el usuario {deletedUserName}");

            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para registrar eventos de auditoría
        private async Task RegisterAuditAsync(string eventType, string description)
        {
            var userId = _userManager.GetUserId(User);
            var userName = User.Identity?.Name ?? "Usuario desconocido";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP no disponible";
            var route = HttpContext.Request.Path.ToString();

            await _auditService.LogAsync(
                eventType,
                description,
                userId,
                userName,
                ipAddress,
                route);
        }
    }
}