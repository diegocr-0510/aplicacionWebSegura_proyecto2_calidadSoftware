using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proyecto2Seguridad.Web.Models;

namespace Proyecto2Seguridad.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tabla de productos
        public DbSet<Product> Products { get; set; }

        // Tabla de auditoría
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}