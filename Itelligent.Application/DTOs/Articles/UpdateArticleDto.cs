using System.ComponentModel.DataAnnotations;

namespace Itelligent.Application.DTOs.Articles;

public class UpdateArticleDto
{
    [Required, MinLength(3)]
    public string Title { get; set; } = string.Empty;

    [Required, MinLength(10)]
    public string Summary { get; set; } = string.Empty;

    [Required, MinLength(10)]
    public string Content { get; set; } = string.Empty;
}
