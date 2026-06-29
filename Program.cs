using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SecureBlog.API.Data;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using SecureBlog.API.Middleware;
using SecureBlog.API.Services.Authorization;
using Microsoft.AspNetCore.Authorization;
using SecureBlog.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog — başlangıç konfigürasyonu, sadece Console sink, minimal
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    //Bilinmeyen bir tip geldiğinde, JsonElement olarak bellekte tut.
                    options.JsonSerializerOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;

                    //Büyük/küçük harf duyarsız property eşleştirme:
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());


                });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDistributedMemoryCache();
//Dikkat!! Sadece MVC için anlamlıdır. API için değil!
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
//Dikkat!! Sadece MVC için anlamlıdır. API için değil!
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//                .AddCookie(option =>
//                {
//                    option.LoginPath = "/accounts/login";

//                });

var config = builder.Configuration;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = config["Jwt:Audience"],
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"])),
                        // ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                        ValidateIssuerSigningKey = true

                    };

                });


/*
 *  Güvenli httpClient konfigürasyonu.
 */

builder.Services.AddHttpClient("SecureBlogClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "SecureBlog/1.0");

}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false, //Dikkat!!! Yanıtlar otomatik yönlendirilemesin!
    MaxConnectionsPerServer = 10
});

var allowedOrigins = builder.Environment.IsDevelopment() ?
                     new[] { "http://localhost:4200", "http://localhost:5137" } :
                     new[] { "https://blog.anadolu.com.tr" };



builder.Services.AddCors(option =>
{
    //option.AddPolicy("PublicPolicy", policy =>
    //{
    //    policy.AllowAnyOrigin()
    //          .AllowAnyMethod()
    //          .AllowAnyHeader();
    //    //www.anadolusigorta.com.tr
    //    //anadolusigorta.com.tr
    //    //customers.anadolusigorta.com.tr
    //    //anadolusigorta.com.tr:8885
    //});

    option.AddPolicy("Secure", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST")
              .WithHeaders("Content-Type", "Authorization")
              .AllowCredentials();



    });
});


builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("CanEditPost", policy => policy.AddRequirements(new PostEditRequirement()));
    opt.AddPolicy("CanDeletePost", policy => policy.RequireRole("Admin", "Editor"));

});

builder.Services.AddScoped<IAuthorizationHandler, PostEditHandler>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<TokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();




app.UseHttpsRedirection();
https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Strict-Transport-Security
app.UseSecurityHeaders();
app.UseHsts();


app.UseCors("Secure");
app.UseAuthentication();
app.UseAuthorization();




app.UseSession();


//DOM based XSS'e karşı response header:
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self'");
    await next();
});
// Authentication/Authorization middleware kasıtlı olarak eklenmedi 

// CORS politikası kasıtlı olarak eklenmedi
// Rate Limiter kasıtlı olarak eklenmedi 
// Global exception handling middleware kasıtlı olarak eklenmedi 

app.MapControllers();

app.Run();
