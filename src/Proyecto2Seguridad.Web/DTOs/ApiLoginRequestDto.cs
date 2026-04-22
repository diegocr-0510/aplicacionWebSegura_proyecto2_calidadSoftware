using System.ComponentModel.DataAnnotations;

namespace Proyecto2Seguridad.Web.DTOs
{
    public class ApiLoginRequestDto
    {
        // Correo del usuario para autenticación API
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido.")]
        public string Email { get; set; } = string.Empty;

        // Contraseña del usuario para autenticación API
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }
}