using Itelligent.Domain.Entities;
using Itelligent.Domain.Interfaces;
using Itelligent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Itelligent.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(int id) =>
        context.Users.FindAsync(id).AsTask();

    public Task<User?> GetByUsernameAsync(string username) =>
        context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task AddAsync(User user) =>
        await context.Users.AddAsync(user);

    public Task SaveChangesAsync() =>
        context.SaveChangesAsync();
}
