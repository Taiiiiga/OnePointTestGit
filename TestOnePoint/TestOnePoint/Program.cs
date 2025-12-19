using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using TestOnePoint.Data;
using TestOnePoint.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TestOnePointContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TestOnePointContext") ?? throw new InvalidOperationException("Connection string 'TestOnePointContext' not found.")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register password hasher, user service and auth service
builder.Services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
builder.Services.AddScoped<TestOnePoint.Services.IUserService, TestOnePoint.Services.UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT key missing. Configure Jwt:Key in appsettings or user-secrets.");
}
var issuer = builder.Configuration["Jwt:Issuer"] ?? "TestOnePoint";
var audience = builder.Configuration["Jwt:Audience"] ?? "TestOnePointClients";
TokenValidationParameters tokenValidationParameters = new()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    ValidateIssuer = true,
    ValidIssuer = issuer,
    ValidateAudience = true,
    ValidAudience = audience,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(2)
};
builder.Services.AddSingleton(tokenValidationParameters);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{

    options.TokenValidationParameters = tokenValidationParameters;
});

var app = builder.Build();

// --- Apply migrations / create database if missing ---
// Retry loop is useful when DB runs in Docker and may not be ready immediately.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<TestOnePointContext>();

    const int maxAttempts = 8;
    var attempt = 0;
    var delayMs = 3000;

    while (true)
    {
        try
        {
            // This will create the database if it does not exist and apply any pending migrations.
            db.Database.Migrate();
            Console.WriteLine("Database migration applied successfully.");
            break;
        }
        catch (Exception ex)
        {
            attempt++;
            Console.WriteLine($"Database migration attempt {attempt} failed: {ex.Message}");

            if (attempt >= maxAttempts)
            {
                // Échec après plusieurs tentatives — on réémet l'exception pour arrêter le démarrage.
                Console.WriteLine("Max migration attempts reached. Application will stop.");
                throw;
            }

            // Attendre avant de réessayer (utile si Postgres démarre via docker-compose)
            Thread.Sleep(delayMs);
        }
    }
}
// ---------------------------------------------------

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// IMPORTANT : authentication middleware before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();