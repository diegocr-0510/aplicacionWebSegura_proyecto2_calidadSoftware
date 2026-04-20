using Microsoft.AspNetCore.Identity;

namespace Proyecto2Seguridad.Web.Models
{
    /// <summary>
    /// Modelo de rol de la aplicación.
    /// Hereda de IdentityRole para administrar roles como SuperAdmin, Auditor y Registrador.
    /// </summary>
    public class ApplicationRole : IdentityRole
    {
    }
}