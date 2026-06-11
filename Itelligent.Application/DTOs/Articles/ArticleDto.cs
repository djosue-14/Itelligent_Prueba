namespace Itelligent.Application.DTOs.Articles;

public class ArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public int AuthorId { get; set; }
}
