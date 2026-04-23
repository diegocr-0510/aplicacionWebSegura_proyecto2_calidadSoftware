using System.ComponentModel.DataAnnotations;

namespace Proyecto2Seguridad.Web.DTOs
{
    public class ApiLoginRequestDto
    {
        // Correo del usuario para login API
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        public string Email { get; set; } = string.Empty;

        // Contraseña del usuario
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }
}