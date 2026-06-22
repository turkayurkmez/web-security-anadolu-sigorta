using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureBlog.API.Data;
using SecureBlog.API.DTOs;
using SecureBlog.API.Models;

namespace SecureBlog.API.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CommentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{postId}")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetByPost(int postId)
    {
        var comments = await _context.Comments
            .Where(c => c.PostId == postId)
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost]
    public async Task<ActionResult<Comment>> Create(CreateCommentDto dto)
    {
        // Validation yok — M4'te eklenecek
        var comment = new Comment
        {
            Content = dto.Content,
            PostId = dto.PostId,
            AuthorId = dto.AuthorId
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return Ok(comment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Yetki kontrolü yok — M12'de eklenecek
        var comment = await _context.Comments.FindAsync(id);
        if (comment is null)
        {
            return NotFound();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
