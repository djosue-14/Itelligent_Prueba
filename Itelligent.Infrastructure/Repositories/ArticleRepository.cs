using Itelligent.Domain.Entities;
using Itelligent.Domain.Interfaces;
using Itelligent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Itelligent.Infrastructure.Repositories;

public class ArticleRepository(AppDbContext context) : IArticleRepository
{
    public async Task<(IEnumerable<Article> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
    {
        var query = context.Articles
            .Include(a => a.Author)
            .OrderByDescending(a => a.PublishedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<Article?> GetByIdAsync(int id) =>
        context.Articles
            .Include(a => a.Author)
            .Include(a => a.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task AddAsync(Article article) =>
        await context.Articles.AddAsync(article);

    public Task SaveChangesAsync() =>
        context.SaveChangesAsync();

    public Task DeleteAsync(Article article)
    {
        context.Articles.Remove(article);
        return Task.CompletedTask;
    }
}
