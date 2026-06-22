using Microsoft.EntityFrameworkCore;
using SecureBlog.API.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog — başlangıç konfigürasyonu, sadece Console sink, minimal
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// UseHttpsRedirection() kasıtlı olarak eklenmedi — M3'te eklenecek
// Authentication/Authorization middleware kasıtlı olarak eklenmedi — M3, M10, M11'de eklenecek
// CORS politikası kasıtlı olarak eklenmedi — M8'de eklenecek
// Rate Limiter kasıtlı olarak eklenmedi — M15'te eklenecek
// Global exception handling middleware kasıtlı olarak eklenmedi — M14'te eklenecek

app.MapControllers();

app.Run();
