namespace Proyecto2Seguridad.Web.DTOs
{
    public class ApiLoginResponseDto
    {
        // Token JWT generado
        public string Token { get; set; } = string.Empty;

        // Fecha y hora de expiración del token
        public DateTime Expiration { get; set; }

        // Nombre del usuario autenticado
        public string UserName { get; set; } = string.Empty;

        // Correo del usuario autenticado
        public string Email { get; set; } = string.Empty;

        // Rol principal del usuario
        public string Role { get; set; } = string.Empty;
    }
}