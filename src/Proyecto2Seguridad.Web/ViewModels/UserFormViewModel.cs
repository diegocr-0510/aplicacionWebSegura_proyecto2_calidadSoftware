using System.ComponentModel.DataAnnotations;

namespace Proyecto2Seguridad.Web.ViewModels
{
    public class UserFormViewModel
    {
        // Id del usuario.
        // Se usa principalmente al editar.
        public string? Id { get; set; }

        // Nombre de usuario para login o identificación.
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede superar los 50 caracteres.")]
        public string UserName { get; set; } = string.Empty;

        // Correo del usuario.
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido.")]
        public string Email { get; set; } = string.Empty;

        // Contraseña.
        // En edición la vamos a dejar opcional para no obligar a cambiarla siempre.
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string? Password { get; set; }

        // Confirmación de contraseña para validar que coincidan.
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La confirmación de contraseña no coincide.")]
        public string? ConfirmPassword { get; set; }

        // Rol asignado al usuario.
        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        public string Role { get; set; } = string.Empty;

        // Fecha del último login.
        // Esto se mostrará en la vista, no se edita manualmente.
        public DateTime? LastLoginAt { get; set; }
    }
}