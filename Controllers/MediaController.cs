using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureBlog.API.Data;
using SecureBlog.API.Models;

namespace SecureBlog.API.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public MediaController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<Media>> Upload(IFormFile file)
    {
        // İzin verilen tip kontrolü yok — M6'da eklenecek
        // Path izolasyonu yok — M6 ve M17'de eklenecek
        var uploadsPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsPath);

        var savePath = Path.Combine(uploadsPath, file.FileName);

        using (var stream = new FileStream(savePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var media = new Media
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            FilePath = savePath,
            FileSize = file.Length,
            UploadedById = 1
        };

        _context.Media.Add(media);
        await _context.SaveChangesAsync();

        return Ok(media);
    }

    [HttpGet("{filename}")]
    public IActionResult GetFile(string filename)
    {
        // Path traversal koruması yok — M17'de eklenecek
        var uploadsPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
        var filePath = Path.Combine(uploadsPath, filename);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var bytes = System.IO.File.ReadAllBytes(filePath);
        return File(bytes, "application/octet-stream", filename);
    }
}
