namespace SecureBlog.API.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAd { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; } = false;

        public string? ReplacedByToken { get; set; }

    }
}
