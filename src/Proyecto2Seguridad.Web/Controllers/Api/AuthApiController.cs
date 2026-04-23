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
        private readonly LoginRateLimitService _loginRateLimitService;

        public AuthApiController(
            UserManager<ApplicationUser> userManager,
            JwtTokenService jwtTokenService,
            AuditService auditService,
            LoginRateLimitService loginRateLimitService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _auditService = auditService;
            _loginRateLimitService = loginRateLimitService;
        }

        // Login de la API para generar JWT
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ApiLoginRequestDto model)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP no disponible";
            var route = HttpContext.Request.Path.ToString();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Clave de bloqueo basada en correo + IP
            var rateLimitKey = $"{model.Email}|{ipAddress}|API";

            // Verificar bloqueo
            if (_loginRateLimitService.IsBlocked(rateLimitKey, out var blockedUntil))
            {
                await _auditService.LogAsync(
                    "API_LOGIN_BLOCKED",
                    $"Login API bloqueado temporalmente para {model.Email} desde IP {ipAddress} hasta {blockedUntil:yyyy-MM-dd HH:mm:ss} UTC",
                    null,
                    model.Email,
                    ipAddress,
                    route);

                return Unauthorized(new
                {
                    message = "Acceso temporalmente bloqueado por múltiples intentos fallidos. Intenta nuevamente más tarde."
                });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                var failureResult = _loginRateLimitService.RegisterFailure(rateLimitKey);

                await _auditService.LogAsync(
                    failureResult.blockedNow ? "API_LOGIN_BLOCKED" : "API_LOGIN_FAILED",
                    failureResult.blockedNow
                        ? $"Se bloqueó el login API para {model.Email} desde IP {ipAddress} por múltiples intentos fallidos."
                        : $"Intento fallido de login API con correo no encontrado: {model.Email}",
                    null,
                    model.Email,
                    ipAddress,
                    route);

                return Unauthorized(new { message = "Credenciales inválidas." });
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!passwordValid)
            {
                var failedAttempt = _loginRateLimitService.RegisterFailure(rateLimitKey);

                await _auditService.LogAsync(
                    failedAttempt.blockedNow ? "API_LOGIN_BLOCKED" : "API_LOGIN_FAILED",
                    failedAttempt.blockedNow
                        ? $"Se bloqueó el login API para el usuario {user.UserName} desde IP {ipAddress} por múltiples intentos fallidos."
                        : $"Intento fallido de login API para el usuario {user.UserName}",
                    user.Id,
                    user.UserName,
                    ipAddress,
                    route);

                return Unauthorized(new { message = "Credenciales inválidas." });
            }

            // Reiniciar contador si el login fue correcto
            _loginRateLimitService.Reset(rateLimitKey);

            var (token, expiration, role) = await _jwtTokenService.GenerateTokenAsync(user);

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