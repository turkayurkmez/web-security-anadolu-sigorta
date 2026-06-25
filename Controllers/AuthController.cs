using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureBlog.API.Data;
using SecureBlog.API.DTOs;
using SecureBlog.API.Models;

namespace SecureBlog.API.Controllers;

// [Authorize] kasıtlı olarak eklenmedi
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u=>u.Email == dto.Email || u.Username == dto.Username))
        {
            return Conflict("Bu isim ya da eposta zaten kullanılıyor");
        }
        // Parola kasıtlı olarak plain text kaydediliyor 
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = dto.Password,
            Role = "Author"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<User>> Login(LoginDto dto)
    {
        // Plain text karşılaştırma 
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username && u.PasswordHash == dto.Password);

        if (user is null)
        {
            return Unauthorized("Hatalı giriş bilgileri");
        }

        HttpContext.Session.Clear();
        await HttpContext.Session.CommitAsync();

        //https://owasp.org/www-community/controls/Session_Fixation_Protection

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserRole", user.Role);



        return Ok(user);
    }
}
