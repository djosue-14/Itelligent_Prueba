namespace Itelligent.Domain.Entities;

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = [];
}
