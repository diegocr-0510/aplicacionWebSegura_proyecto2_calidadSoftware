namespace Proyecto2Seguridad.Web.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        // Tipo del evento registrado
        public string EventType { get; set; } = string.Empty;

        // Descripción legible del evento
        public string Description { get; set; } = string.Empty;

        // Id del usuario relacionado al evento
        public string? UserId { get; set; }

        // Nombre de usuario relacionado al evento
        public string? UserName { get; set; }

        // Dirección IP de origen
        public string IpAddress { get; set; } = string.Empty;

        // Ruta donde ocurrió el evento
        public string Route { get; set; } = string.Empty;

        // Fecha y hora del evento
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}