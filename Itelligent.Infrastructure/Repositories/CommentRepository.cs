using Itelligent.Domain.Entities;
using Itelligent.Domain.Interfaces;
using Itelligent.Infrastructure.Data;

namespace Itelligent.Infrastructure.Repositories;

public class CommentRepository(AppDbContext context) : ICommentRepository
{
    public async Task AddAsync(Comment comment) =>
        await context.Comments.AddAsync(comment);

    public Task SaveChangesAsync() =>
        context.SaveChangesAsync();
}
