using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;
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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
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
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();