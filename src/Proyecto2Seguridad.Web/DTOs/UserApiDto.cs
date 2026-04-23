using System.ComponentModel.DataAnnotations;

namespace Proyecto2Seguridad.Web.DTOs
{
    public class UserApiDto
    {
        // Id del usuario
        public string? Id { get; set; }

        // Nombre de usuario
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede superar los 50 caracteres.")]
        public string UserName { get; set; } = string.Empty;

        // Correo del usuario
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido.")]
        public string Email { get; set; } = string.Empty;

        // Contraseña del usuario
        // Al editar puede ir vacía si no se desea cambiar
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string? Password { get; set; }

        // Rol del usuario
        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string Role { get; set; } = string.Empty;

        // Último login
        public DateTime? LastLoginAt { get; set; }
    }
}