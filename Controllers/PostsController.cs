using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureBlog.API.Data;
using SecureBlog.API.DTOs;
using SecureBlog.API.Models;
using System.Text.Encodings.Web;
using System.Xml;

namespace SecureBlog.API.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PostsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Post>>> GetAll()
    {
        return Ok(await _context.Posts.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetById(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        //örnek:
        var safeTitle = HtmlEncoder.Default.Encode(post.Title);
        /*
         * Eğer db'deki Title XSS içeriyorsa <script> alert('Saldırı :)') </script>
         * &lt;script&bt; 
         */

        if (post is null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Post>>> Search([FromQuery] string keyword)
    {
        // Kasıtlı zafiyetli: ham string birleştirmeyle SQL sorgusu 
        //var sql = $"SELECT * FROM Posts WHERE Title LIKE '%{keyword}%'";
        //var posts = await _context.Posts.FromSqlRaw(sql).ToListAsync();

        // Güvenli: 

        if (string.IsNullOrWhiteSpace(keyword))
        {
             return BadRequest("keyword boş olamaz");            
        }

        //EF.Functions.Like aracılığıyle güvenli parametrik sorgu:
        var posts = await _context.Posts
            .Where(p => EF.Functions.Like(p.Title, $"%{keyword}%"))
            .AsNoTracking()
            .ToListAsync();

        return Ok(posts);
    }

    [HttpPost]
    public async Task<ActionResult<Post>> Create(CreatePostDto dto, IValidator<CreatePostDto> validator)
    {

        //FluentValidation ile denetim:
        var result = validator.Validate(dto);       
        if (!result.IsValid)
        {
            return BadRequest(result.ToDictionary());
        }


        
        var post = new Post
        {
            Title = dto.Title,
            Content = dto.Content,
            Slug = dto.Title.ToLowerInvariant().Replace(' ', '-'),
            AuthorId = dto.AuthorId,
            IsPublished = false
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreatePostDto dto)
    {
        // Yetki kontrolü yok 
        var post = await _context.Posts.FindAsync(id);
        if (post is null)
        {
            return NotFound();
        }

        post.Title = dto.Title;
        post.Content = dto.Content;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Yetki kontrolü yok 
        var post = await _context.Posts.FindAsync(id);
        if (post is null)
        {
            return NotFound();
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return NoContent();
    }


    //Zafiyetli XML request'i alan bir end point!!!
    [HttpPost("import-xml")]
    public IActionResult ImportFromXML([FromBody]string xmlContent)
    {
        var document = new XmlDocument();
        document.LoadXml(xmlContent);
        var titleNode = document.SelectSingleNode("//title");
        var contentNode = document.SelectSingleNode("//content");

        if (titleNode is null || contentNode is null)
        {
            return BadRequest("XML Formatı geçersiz");
        }
        return Ok(new { Title = titleNode.InnerText, Content = contentNode.InnerText });
    }
}
