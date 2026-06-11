using Itelligent.Domain.Entities;

namespace Itelligent.Domain.Interfaces;

public interface ICommentRepository
{
    Task AddAsync(Comment comment);
    Task SaveChangesAsync();
}
