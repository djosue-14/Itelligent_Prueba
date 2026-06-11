using Itelligent.Domain.Entities;
using Itelligent.Infrastructure.Data;
using Itelligent.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Itelligent.Tests.Infrastructure;

public class ArticleRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ArticleRepository _repository;

    public ArticleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new ArticleRepository(_context);

        SeedData();
    }

    private void SeedData()
    {
        var author = new User { Id = 2, Username = "testuser", PasswordHash = "hash", Role = Domain.Enums.Role.User };
        _context.Users.Add(author);

        _context.Articles.AddRange(
            new Article { Id = 1, Title = "Article A", Summary = "Summary A", Content = "Content A", PublishedAt = DateTime.UtcNow.AddDays(-2), AuthorId = 2 },
            new Article { Id = 2, Title = "Article B", Summary = "Summary B", Content = "Content B", PublishedAt = DateTime.UtcNow.AddDays(-1), AuthorId = 2 },
            new Article { Id = 3, Title = "Article C", Summary = "Summary C", Content = "Content C", PublishedAt = DateTime.UtcNow, AuthorId = 2,
                Comments = [new Comment { Id = 1, Text = "Great!", UserId = 2, PublishedAt = DateTime.UtcNow }] }
        );

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAll_ShouldReturnPaginatedResults()
    {
        // Page 1 with size 2 should return 2 items, total 3
        var (items, total) = await _repository.GetAllAsync(page: 1, pageSize: 2);

        Assert.Equal(3, total);
        Assert.Equal(2, items.Count());
    }

    [Fact]
    public async Task GetAll_Page2_ShouldReturnRemainingItems()
    {
        var (items, total) = await _repository.GetAllAsync(page: 2, pageSize: 2);

        Assert.Equal(3, total);
        Assert.Single(items);
    }

    [Fact]
    public async Task GetById_WithComments_ShouldIncludeComments()
    {
        var article = await _repository.GetByIdAsync(3);

        Assert.NotNull(article);
        Assert.NotEmpty(article.Comments);
        Assert.Equal("Great!", article.Comments.First().Text);
    }

    [Fact]
    public async Task GetById_NonExistent_ShouldReturnNull()
    {
        var article = await _repository.GetByIdAsync(999);
        Assert.Null(article);
    }

    public void Dispose() => _context.Dispose();
}
