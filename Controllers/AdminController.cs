using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecureBlog.API.Data;

namespace SecureBlog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(AppDbContext _context) : ControllerBase
    {
        [Authorize]
        [HttpGet("users")]       
        public async Task<IActionResult> GetUsers()
        {
            var users = _context.Users.Select(u => new { u.Username, u.Email, u.Role });
            return Ok(users);
        }
    }
}
