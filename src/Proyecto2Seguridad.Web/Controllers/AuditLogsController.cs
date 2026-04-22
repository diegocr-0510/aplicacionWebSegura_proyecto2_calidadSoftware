using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto2Seguridad.Web.Data;

namespace Proyecto2Seguridad.Web.Controllers
{
    // Solo SuperAdmin puede ver auditoría
    [Authorize(Roles = "SuperAdmin")]
    public class AuditLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Muestra los logs ordenados del más reciente al más antiguo
        public async Task<IActionResult> Index()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(logs);
        }
    }
}