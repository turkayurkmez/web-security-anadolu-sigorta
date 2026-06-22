using Microsoft.EntityFrameworkCore;
using SecureBlog.API.Models;

namespace SecureBlog.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Media> Media => Set<Media>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Media>()
            .HasOne(m => m.UploadedBy)
            .WithMany()
            .HasForeignKey(m => m.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@secureblog.local",
                PasswordHash = "admin123", // M9'da hash'lenecek
                Role = "Admin",
                CreatedAt = seedDate
            },
            new User
            {
                Id = 2,
                Username = "author1",
                Email = "author1@secureblog.local",
                PasswordHash = "author123", // M9'da hash'lenecek
                Role = "Author",
                CreatedAt = seedDate
            }
        );

        modelBuilder.Entity<Post>().HasData(
            new Post
            {
                Id = 1,
                Title = "Merhaba SecureBlog",
                Content = "Bu, eğitim için oluşturulmuş ilk makale.",
                Slug = "merhaba-secureblog",
                IsPublished = true,
                AuthorId = 1,
                CreatedAt = seedDate
            },
            new Post
            {
                Id = 2,
                Title = "Güvenlik Eğitimine Giriş",
                Content = "Bu makalede güvenlik katmanlarını adım adım ekleyeceğiz.",
                Slug = "guvenlik-egitimine-giris",
                IsPublished = true,
                AuthorId = 2,
                CreatedAt = seedDate
            },
            new Post
            {
                Id = 3,
                Title = "Taslak Makale",
                Content = "Henüz yayınlanmamış bir taslak.",
                Slug = "taslak-makale",
                IsPublished = false,
                AuthorId = 2,
                CreatedAt = seedDate
            }
        );

        modelBuilder.Entity<Comment>().HasData(
            new Comment
            {
                Id = 1,
                Content = "Harika bir başlangıç!",
                PostId = 1,
                AuthorId = 2,
                CreatedAt = seedDate
            },
            new Comment
            {
                Id = 2,
                Content = "Sabırsızlıkla bekliyorum.",
                PostId = 2,
                AuthorId = 1,
                CreatedAt = seedDate
            }
        );
    }
}
