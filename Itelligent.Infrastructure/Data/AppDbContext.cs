using Itelligent.Domain.Entities;
using Itelligent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Itelligent.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Username).IsRequired().HasMaxLength(100);
            e.Property(u => u.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Article>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Title).IsRequired().HasMaxLength(200);
            e.Property(a => a.Summary).IsRequired().HasMaxLength(500);
            e.Property(a => a.Content).IsRequired();
            e.HasOne(a => a.Author)
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Text).IsRequired().HasMaxLength(1000);
            e.HasOne(c => c.Article)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed default admin — username: admin / password: Admin123!
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = "$2b$11$wOFk93HxqYbu5kt0VtZPUOOdYL4ipOx35.dWyBWREhKrLcoA54F4O",
            Role = Role.Admin
        });
    }
}
