using Itelligent.Application.DTOs.Comments;

namespace Itelligent.Application.DTOs.Articles;

public class ArticleDetailDto : ArticleDto
{
    public List<CommentDto> Comments { get; set; } = [];
}
