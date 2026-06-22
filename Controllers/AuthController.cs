using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureBlog.API.Data;
using SecureBlog.API.DTOs;
using SecureBlog.API.Models;

namespace SecureBlog.API.Controllers;

// [Authorize] kasıtlı olarak eklenmedi — M3'te eklenecek
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
        // Parola kasıtlı olarak plain text kaydediliyor — M9'da hash'lenecek
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
        // Plain text karşılaştırma — M3 ve M11'de düzeltilecek
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username && u.PasswordHash == dto.Password);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }
}
