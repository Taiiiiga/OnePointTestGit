using System;

namespace TestOnePoint.Services
{
    public class JwtOptions
    {
        public string Key { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public string? Audience { get; set; }

        // Durée d'expiration en heures (par défaut 1h)
        public int ExpiresHours { get; set; } = 1;
    }
}
