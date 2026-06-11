using System.ComponentModel.DataAnnotations;

namespace Itelligent.Application.DTOs.Comments;

public class CreateCommentDto
{
    [Required, MinLength(1)]
    public string Text { get; set; } = string.Empty;
}
