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

    //İzin verilen dosyaların Magic bytes değerleri:

    private readonly Dictionary<string, byte[]> _allowedSignatures = new Dictionary<string, byte[]>()
    {
        {"image/jpeg",new byte[]{ 0xFF, 0xD8, 0xFF } },
        {"image/png",new byte[]{ 0x89, 0x50, 0x4E, 0x47 } },
        {"image/webp",new byte[]{ 0x52, 0x49, 0x46, 0x46 } },


    };

    private const long MAX_FILE_SIZE_BYTES = 1024 * 1024 * 5; //5MB
   
    private bool  IsValidImageSignature(IFormFile file, string contentType)
    {
        if (!_allowedSignatures.TryGetValue(contentType, out var expectedBytes))
        {
            return false;
        }

        using var reader = new BinaryReader(file.OpenReadStream());

        var headerBytes = reader.ReadBytes(expectedBytes.Length);

        return headerBytes.SequenceEqual(expectedBytes);
    }


    [HttpPost("upload")]
    public async Task<ActionResult<Media>> Upload(IFormFile file)
    {

        //1. Dosya gerçekten var mı?
        if (file is null || file.Length ==0)
        {
            return BadRequest("Dosya boş olamaz");
        }

        //2. Dosya, izin verilenden büyük olamaz.
        if (file.Length > MAX_FILE_SIZE_BYTES)
        {
            return BadRequest("En fazla 5MB'lik dosya olabilir.");
        }

        //3. dosya content-type izin verilen dosyalarda var mı?
        if (!_allowedSignatures.ContainsKey(file.ContentType))
        {
            return BadRequest("Bu dosya türü desteklenmiyor");
        }

        if (!IsValidImageSignature(file,file.ContentType))
        {
            return BadRequest("Dosya içeriği ile beyan edilen tür aynı değil!");
        }

        var extension = file.ContentType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/webp" => ".webp",
            _ => throw new InvalidOperationException("Desteklenmeyen MIME Type")

        };




        var safeName = Path.GetRandomFileName().Replace(".", "") + extension;

        var uploadsPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsPath);

        var savePath = Path.Combine(uploadsPath, safeName);

        using (var stream = new FileStream(savePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var media = new Media
        {
            FileName = safeName,
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
        // Path traversal koruması yok 
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
