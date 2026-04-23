namespace Proyecto2Seguridad.Web.Services
{
    public class LoginRateLimitService
    {
        // Número máximo de intentos fallidos permitidos
        private const int MaxFailedAttempts = 5;

        // Tiempo de bloqueo en minutos
        private const int BlockMinutes = 5;

        // Diccionario en memoria para registrar intentos fallidos por clave
        private readonly Dictionary<string, LoginAttemptInfo> _attempts = new();

        // Verifica si una clave está bloqueada actualmente
        public bool IsBlocked(string key, out DateTime? blockedUntil)
        {
            blockedUntil = null;

            if (!_attempts.ContainsKey(key))
            {
                return false;
            }

            var info = _attempts[key];

            // Si hay bloqueo vigente, devolver true
            if (info.BlockedUntil.HasValue && info.BlockedUntil.Value > DateTime.UtcNow)
            {
                blockedUntil = info.BlockedUntil;
                return true;
            }

            // Si el bloqueo ya expiró, reiniciar contador
            if (info.BlockedUntil.HasValue && info.BlockedUntil.Value <= DateTime.UtcNow)
            {
                _attempts.Remove(key);
                return false;
            }

            return false;
        }

        // Registra un intento fallido
        public (bool blockedNow, DateTime? blockedUntil, int failedCount) RegisterFailure(string key)
        {
            if (!_attempts.ContainsKey(key))
            {
                _attempts[key] = new LoginAttemptInfo();
            }

            var info = _attempts[key];
            info.FailedCount++;

            // Si llega al máximo, bloquear
            if (info.FailedCount >= MaxFailedAttempts)
            {
                info.BlockedUntil = DateTime.UtcNow.AddMinutes(BlockMinutes);
                return (true, info.BlockedUntil, info.FailedCount);
            }

            return (false, null, info.FailedCount);
        }

        // Reinicia intentos después de login exitoso
        public void Reset(string key)
        {
            if (_attempts.ContainsKey(key))
            {
                _attempts.Remove(key);
            }
        }

        // Clase interna para guardar estado del login
        private class LoginAttemptInfo
        {
            public int FailedCount { get; set; } = 0;
            public DateTime? BlockedUntil { get; set; }
        }
    }
}