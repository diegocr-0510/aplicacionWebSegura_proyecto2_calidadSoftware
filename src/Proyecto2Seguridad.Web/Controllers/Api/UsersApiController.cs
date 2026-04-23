using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto2Seguridad.Web.DTOs;
using Proyecto2Seguridad.Web.Models;
using Proyecto2Seguridad.Web.Services;

namespace Proyecto2Seguridad.Web.Controllers.Api
{
    [ApiController]
    [Route("api/users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin")]
    public class UsersApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditService _auditService;

        public UsersApiController(
            UserManager<ApplicationUser> userManager,
            AuditService auditService)
        {
            _userManager = userManager;
            _auditService = auditService;
        }

        // Obtener todos los usuarios
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userManager.Users.ToList();

            var result = new List<UserApiDto>();

            foreach (var user in users)
            {
                // Obtener rol del usuario
                var roles = _userManager.GetRolesAsync(user).Result;
                var currentRole = roles.FirstOrDefault() ?? "Sin rol";

                result.Add(new UserApiDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Role = currentRole,
                    LastLoginAt = user.LastLoginAt
                });
            }

            return Ok(result);
        }

        // Obtener usuario por id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault() ?? "Sin rol";

            var result = new UserApiDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = currentRole,
                LastLoginAt = user.LastLoginAt
            };

            return Ok(result);
        }

        // Crear usuario
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserApiDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // La contraseña es obligatoria al crear
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest(new { message = "La contraseña es obligatoria." });
            }

            // Validar username único
            var existingUserByUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByUserName != null)
            {
                return BadRequest(new { message = "Ya existe un usuario con ese nombre." });
            }

            // Validar email único
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest(new { message = "Ya existe un usuario con ese correo." });
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);

            if (!createResult.Succeeded)
            {
                return BadRequest(createResult.Errors);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            await RegisterAuditAsync(
                "API_USER_CREATE",
                $"Se creó por API el usuario {user.UserName} con rol {model.Role}");

            model.Id = user.Id;
            model.Password = null;
            model.LastLoginAt = user.LastLoginAt;

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, model);
        }

        // Editar usuario
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UserApiDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            // Validar username único
            var existingUserByUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByUserName != null && existingUserByUserName.Id != user.Id)
            {
                return BadRequest(new { message = "Ya existe otro usuario con ese nombre." });
            }

            // Validar email único
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
            {
                return BadRequest(new { message = "Ya existe otro usuario con ese correo." });
            }

            user.UserName = model.UserName;
            user.Email = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors);
            }

            // Cambiar contraseña si viene informada
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

                if (!passwordResult.Succeeded)
                {
                    return BadRequest(passwordResult.Errors);
                }
            }

            // Reasignar rol
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            await RegisterAuditAsync(
                "API_USER_EDIT",
                $"Se editó por API el usuario {user.UserName} y se asignó rol {model.Role}");

            return Ok(new { message = "Usuario actualizado correctamente." });
        }

        // Eliminar usuario
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            // Evitar que el usuario actual se elimine a sí mismo
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
            if (user.Id == currentUserId)
            {
                return BadRequest(new { message = "No puedes eliminar tu propio usuario autenticado." });
            }

            var userName = user.UserName ?? string.Empty;

            var deleteResult = await _userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
            {
                return BadRequest(deleteResult.Errors);
            }

            await RegisterAuditAsync(
                "API_USER_DELETE",
                $"Se eliminó por API el usuario {userName}");

            return Ok(new { message = "Usuario eliminado correctamente." });
        }

        // Registrar auditoría de eventos API de usuarios
        private async Task RegisterAuditAsync(string eventType, string description)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
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