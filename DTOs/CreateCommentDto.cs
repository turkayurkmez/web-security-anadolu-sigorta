namespace SecureBlog.API.DTOs;

public class CreateCommentDto
{
    public string Content { get; set; } = string.Empty;
    public int PostId { get; set; }
    public int AuthorId { get; set; }
}
