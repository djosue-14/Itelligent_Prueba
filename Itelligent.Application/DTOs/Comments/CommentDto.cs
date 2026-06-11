namespace Itelligent.Application.DTOs.Comments;

public class CommentDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string Username { get; set; } = string.Empty;
}
