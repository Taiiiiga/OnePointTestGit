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

// Register password hasher and user service
builder.Services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
builder.Services.AddScoped<TestOnePoint.Services.IUserService, TestOnePoint.Services.UserService>();

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