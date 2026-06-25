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

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();




app.UseHttpsRedirection();
app.UseHsts();
app.UseSession();
// Authentication/Authorization middleware kasıtlı olarak eklenmedi 
// CORS politikası kasıtlı olarak eklenmedi
// Rate Limiter kasıtlı olarak eklenmedi 
// Global exception handling middleware kasıtlı olarak eklenmedi 

app.MapControllers();

app.Run();
