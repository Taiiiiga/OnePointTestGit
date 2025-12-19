using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestOnePoint.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TestOnePointContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TestOnePointContext") ?? throw new InvalidOperationException("Connection string 'TestOnePointContext' not found.")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<TestOnePoint.Services.IUserService, TestOnePoint.Services.UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
