using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto2Seguridad.Web.Models;
using System.Diagnostics;

namespace Proyecto2Seguridad.Web.Controllers
{
    
    /// Controlador principal protegido con autenticación.
    
    [Authorize]
    public class HomeController : Controller
    {
        
        /// Página principal del sistema.
        
        public IActionResult Index()
        {
            return View();
        }

        
        /// Página de privacidad.
        
        public IActionResult Privacy()
        {
            return View();
        }

        
        /// Página de error.
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}