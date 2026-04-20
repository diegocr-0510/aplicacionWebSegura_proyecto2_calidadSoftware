using System.ComponentModel.DataAnnotations;

namespace Proyecto2Seguridad.Web.ViewModels
{
    /// <summary>
    /// ViewModel usado para el formulario de inicio de sesión.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Permite recordar la sesión del usuario.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}