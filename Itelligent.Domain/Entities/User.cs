using Itelligent.Domain.Enums;

namespace Itelligent.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }
    public ICollection<Article> Articles { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}
