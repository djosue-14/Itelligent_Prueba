using Itelligent.Domain.Entities;

namespace Itelligent.Domain.Interfaces;

public interface IArticleRepository
{
    Task<(IEnumerable<Article> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
    Task<Article?> GetByIdAsync(int id);
    Task AddAsync(Article article);
    Task SaveChangesAsync();
    Task DeleteAsync(Article article);
}
