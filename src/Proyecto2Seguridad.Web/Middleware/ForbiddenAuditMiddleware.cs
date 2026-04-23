using Microsoft.AspNetCore.Identity;
using Proyecto2Seguridad.Web.Models;
using Proyecto2Seguridad.Web.Services;

namespace Proyecto2Seguridad.Web.Middleware
{
    public class ForbiddenAuditMiddleware
    {
        private readonly RequestDelegate _next;

        public ForbiddenAuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            AuditService auditService,
            UserManager<ApplicationUser> userManager)
        {
            // Ejecutar siguiente componente del pipeline
            await _next(context);

            // Solo registrar si la respuesta fue 403
            if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                var user = await userManager.GetUserAsync(context.User);
                var userId = user?.Id;
                var userName = user?.UserName ?? "Usuario no identificado";
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "IP no disponible";
                var route = context.Request.Path.ToString();

                await auditService.LogAsync(
                    "ACCESS_DENIED",
                    $"Acceso denegado a la ruta {route}",
                    userId,
                    userName,
                    ipAddress,
                    route);
            }
        }
    }
}