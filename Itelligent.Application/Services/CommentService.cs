using AutoMapper;
using Itelligent.Application.DTOs.Comments;
using Itelligent.Application.Exceptions;
using Itelligent.Domain.Entities;
using Itelligent.Domain.Interfaces;

namespace Itelligent.Application.Services;

public class CommentService(ICommentRepository commentRepository, IArticleRepository articleRepository, IMapper mapper)
{
    public async Task<CommentDto> AddCommentAsync(int articleId, int userId, CreateCommentDto dto)
    {
        var article = await articleRepository.GetByIdAsync(articleId)
            ?? throw new NotFoundException($"Article {articleId} not found.");

        var comment = new Comment
        {
            Text = dto.Text,
            PublishedAt = DateTime.UtcNow,
            ArticleId = article.Id,
            UserId = userId
        };

        await commentRepository.AddAsync(comment);
        await commentRepository.SaveChangesAsync();

        return mapper.Map<CommentDto>(comment);
    }
}
