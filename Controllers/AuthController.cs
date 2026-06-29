using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureBlog.API.Data;
using SecureBlog.API.DTOs;
using SecureBlog.API.Models;
using SecureBlog.API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureBlog.API.Controllers;

// [Authorize] kasıtlı olarak eklenmedi
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, IConfiguration configuration, ILogger<AuthController> logger )
    {
        _context = context;
        _config = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u=>u.Email == dto.Email || u.Username == dto.Username))
        {
            return Conflict("Bu isim ya da eposta zaten kullanılıyor");
        }



        // Parola kasıtlı olarak plain text kaydediliyor 

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);


        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = passwordHash,
            Role = "Author"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<User>> Login(LoginDto dto, TokenService tokenService)
    {
        // Plain text karşılaştırma 
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password,user.PasswordHash))
        {
            return Unauthorized("Hatalı giriş bilgileri");
        }



        HttpContext.Session.Clear();
        await HttpContext.Session.CommitAsync();

        //https://owasp.org/www-community/controls/Session_Fixation_Protection

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserRole", user.Role);

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = new RefreshToken()
        {
            Token = tokenService.GenerateRefreshToken(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();



        return Ok(new { token = accessToken, refreshToken = refreshToken.Token });
    }
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenDto dto, TokenService tokenService)
    {
        var stored = await _context.RefreshTokens.Include(rt => rt.User)
                                                 .FirstOrDefaultAsync(rt => rt.Token == dto.Token);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized("Bu token geçersiz");

        }

        stored.IsRevoked = true;
        stored.ReplacedByToken = tokenService.GenerateRefreshToken();

        var newRefreshtoken = new RefreshToken()
        {
            User = stored.User,
            Token = stored.ReplacedByToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _context.RefreshTokens.AddAsync(newRefreshtoken);
        await _context.SaveChangesAsync();

        var newACessToken = tokenService.GenerateAccessToken(stored.User);

        return Ok(new { acess = newACessToken, refresh = newRefreshtoken.Token });

    }






}

public class RefreshTokenDto
{
    public string Token { get; internal set; }
}