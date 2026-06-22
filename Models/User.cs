namespace SecureBlog.API.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;  // M9'da doldurulacak
    public string Role { get; set; } = "Author";              // Admin | Editor | Author | Reader
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
