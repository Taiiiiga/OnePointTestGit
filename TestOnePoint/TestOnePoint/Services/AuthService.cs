using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TestOnePoint.Data;

namespace TestOnePoint.Services
{
    public class AuthService : IAuthService
    {
        private readonly TestOnePointContext context;
        private readonly IPasswordHasher hasher;
        private readonly TokenValidationParameters options;

        public AuthService(TestOnePointContext context, IPasswordHasher hasher, TokenValidationParameters options)
        {
            this.context = context;
            this.hasher = hasher;
            this.options = options;
        }

        public async Task<string?> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return null;

            if (!hasher.Verify(password, user.PasswordHash))
                return null;

            var issuerSigningKey = options.IssuerSigningKey;
            if (issuerSigningKey == null)
                throw new InvalidOperationException("JWT key not configured (Jwt:Key).");

            // Use the exact configured SecurityKey for signing so validation can find the matching key.
            if (issuerSigningKey is not SymmetricSecurityKey signingKey)
                throw new InvalidOperationException("Configured IssuerSigningKey must be a SymmetricSecurityKey for this signer.");

            var issuer = options.ValidIssuer ?? "TestOnePoint";
            var audience = options.ValidAudience ?? "TestOnePointClients";
            var expiresHours = 1;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.Name ?? string.Empty),   
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(expiresHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}