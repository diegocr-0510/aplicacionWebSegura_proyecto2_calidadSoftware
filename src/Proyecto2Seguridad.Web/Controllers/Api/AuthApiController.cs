using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto2Seguridad.Web.DTOs;
using Proyecto2Seguridad.Web.Models;
using Proyecto2Seguridad.Web.Services;

namespace Proyecto2Seguridad.Web.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly AuditService _auditService;

        public AuthApiController(
            UserManager<ApplicationUser> userManager,
            JwtTokenService jwtTokenService,
            AuditService auditService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _auditService = auditService;
        }

        // Login de la API para generar JWT
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ApiLoginRequestDto model)
        {
            // Obtener IP y ruta
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP no disponible";
            var route = HttpContext.Request.Path.ToString();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Buscar usuario por correo
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                await _auditService.LogAsync(
                    "API_LOGIN_FAILED",
                    $"Intento fallido de login API con correo no encontrado: {model.Email}",
                    null,
                    model.Email,
                    ipAddress,
                    route);

                return Unauthorized(new { message = "Credenciales inválidas." });
            }

            // Validar contraseña
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!passwordValid)
            {
                await _auditService.LogAsync(
                    "API_LOGIN_FAILED",
                    $"Intento fallido de login API para el usuario {user.UserName}",
                    user.Id,
                    user.UserName,
                    ipAddress,
                    route);

                return Unauthorized(new { message = "Credenciales inválidas." });
            }

            // Generar token JWT
            var (token, expiration, role) = await _jwtTokenService.GenerateTokenAsync(user);

            // Registrar login API exitoso
            await _auditService.LogAsync(
                "API_LOGIN_SUCCESS",
                $"Login API exitoso del usuario {user.UserName}",
                user.Id,
                user.UserName,
                ipAddress,
                route);

            var response = new ApiLoginResponseDto
            {
                Token = token,
                Expiration = expiration,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = role
            };

            return Ok(response);
        }
    }
}