namespace Itelligent.Domain.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
