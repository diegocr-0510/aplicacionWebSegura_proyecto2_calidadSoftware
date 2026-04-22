using Proyecto2Seguridad.Web.Data;
using Proyecto2Seguridad.Web.Models;

namespace Proyecto2Seguridad.Web.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Registra un evento de auditoría en la base de datos
        public async Task LogAsync(
            string eventType,
            string description,
            string? userId,
            string? userName,
            string ipAddress,
            string route)
        {
            var log = new AuditLog
            {
                EventType = eventType,
                Description = description,
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                Route = route,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}