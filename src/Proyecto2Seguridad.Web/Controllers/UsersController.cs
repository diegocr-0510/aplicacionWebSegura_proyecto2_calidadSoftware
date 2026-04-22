using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto2Seguridad.Web.Models;
using Proyecto2Seguridad.Web.ViewModels;

namespace Proyecto2Seguridad.Web.Controllers
{
    // Solo el SuperAdmin puede administrar usuarios.
    [Authorize(Roles = "SuperAdmin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Muestra el listado de usuarios.
        public async Task<IActionResult> Index()
        {
            // Obtener todos los usuarios del sistema.
            var users = _userManager.Users.ToList();

            // Lista que vamos a enviar a la vista.
            var userList = new List<UserFormViewModel>();

            foreach (var user in users)
            {
                // Obtener el rol actual del usuario.
                var roles = await _userManager.GetRolesAsync(user);
                var currentRole = roles.FirstOrDefault() ?? "Sin rol";

                // Agregar los datos al ViewModel.
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

        // Muestra el formulario para crear un usuario.
        [HttpGet]
        public IActionResult Create()
        {
            // Cargar los roles para mostrarlos en el formulario.
            ViewBag.Roles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r != null)
                .ToList();

            return View();
        }

        // Guarda un nuevo usuario.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            // Volver a cargar roles por si hay error de validación.
            ViewBag.Roles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r != null)
                .ToList();

            // Validación extra: la contraseña es obligatoria al crear.
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "La contraseña es obligatoria.");
            }

            // Validar que el nombre de usuario no exista.
            var existingUserByUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByUserName != null)
            {
                ModelState.AddModelError("UserName", "Ya existe un usuario con ese nombre.");
            }

            // Validar que el correo no exista.
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Email", "Ya existe un usuario con ese correo.");
            }

            // Si hay errores, volver a la vista.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Crear la entidad de usuario.
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = true
            };

            // Crear el usuario con la contraseña segura.
            var result = await _userManager.CreateAsync(user, model.Password!);

            if (!result.Succeeded)
            {
                // Si Identity devuelve errores, agregarlos al ModelState.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            // Asignar el rol seleccionado.
            await _userManager.AddToRoleAsync(user, model.Role);

            return RedirectToAction(nameof(Index));
        }

        // Muestra el formulario de edición.
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            // Buscar usuario.
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Obtener rol actual.
            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault() ?? string.Empty;

            // Cargar roles para el dropdown.
            ViewBag.Roles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r != null)
                .ToList();

            // Pasar datos al formulario.
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

        // Guarda los cambios de un usuario.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            // Volver a cargar roles por si hay error.
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

            // Buscar usuario existente.
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            // Validar que no exista otro usuario con el mismo nombre.
            var existingUserByUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByUserName != null && existingUserByUserName.Id != user.Id)
            {
                ModelState.AddModelError("UserName", "Ya existe otro usuario con ese nombre.");
                return View(model);
            }

            // Validar que no exista otro usuario con el mismo correo.
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
            {
                ModelState.AddModelError("Email", "Ya existe otro usuario con ese correo.");
                return View(model);
            }

            // Actualizar datos básicos.
            user.UserName = model.UserName;
            user.Email = model.Email;

            // Guardar cambios del usuario.
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            // Si se escribió nueva contraseña, actualizarla.
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                // Generar token para cambiar contraseña de forma segura.
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

            // Actualizar rol.
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            return RedirectToAction(nameof(Index));
        }

        // Muestra confirmación de eliminación.
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            // Buscar usuario.
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Obtener rol actual.
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

        // Elimina un usuario del sistema.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            // Buscar usuario.
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Evitar que el admin actual se elimine a sí mismo por accidente.
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "No puedes eliminar tu propio usuario mientras estás autenticado.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}