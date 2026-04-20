using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proyecto2Seguridad.Web.Models;

namespace Proyecto2Seguridad.Web.Data
{
    /// <summary>
    /// Contexto principal de Entity Framework Core.
    /// Aquí se registran las tablas de la aplicación y las tablas de Identity.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        /// <summary>
        /// Constructor del contexto.
        /// </summary>
        /// <param name="options">Opciones de configuración del contexto.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
