using Microsoft.AspNetCore.Identity;

namespace Proyecto2Seguridad.Web.Models
{
    /// <summary>
    /// Modelo de usuario de la aplicación.
    /// Hereda de IdentityUser para reutilizar la seguridad de ASP.NET Core Identity.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Fecha y hora del último inicio de sesión exitoso.
        /// Esto ayuda a cumplir con el requerimiento de mostrar el último login.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }
}