using AutoMapper;
using Itelligent.Application.DTOs.Articles;
using Itelligent.Application.Exceptions;
using Itelligent.Domain.Entities;
using Itelligent.Domain.Enums;
using Itelligent.Domain.Interfaces;

namespace Itelligent.Application.Services;

public class ArticleService(IArticleRepository articleRepository, IMapper mapper)
{
    public async Task<PagedArticlesDto> GetAllAsync(int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await articleRepository.GetAllAsync(page, pageSize);
        return new PagedArticlesDto
        {
            Items = mapper.Map<IEnumerable<ArticleDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ArticleDetailDto> GetByIdAsync(int id)
    {
        var article = await articleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Article {id} not found.");

        return mapper.Map<ArticleDetailDto>(article);
    }

    public async Task<ArticleDto> CreateAsync(CreateArticleDto dto, int authorId)
    {
        var article = new Article
        {
            Title = dto.Title,
            Summary = dto.Summary,
            Content = dto.Content,
            PublishedAt = DateTime.UtcNow,
            AuthorId = authorId
        };

        await articleRepository.AddAsync(article);
        await articleRepository.SaveChangesAsync();

        var created = await articleRepository.GetByIdAsync(article.Id)
            ?? throw new NotFoundException("Created article not found.");

        return mapper.Map<ArticleDto>(created);
    }

    public async Task<ArticleDto> UpdateAsync(int id, UpdateArticleDto dto, int userId, Role role)
    {
        var article = await articleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Article {id} not found.");

        if (article.AuthorId != userId && role != Role.Admin)
            throw new ForbiddenException();

        article.Title = dto.Title;
        article.Summary = dto.Summary;
        article.Content = dto.Content;

        await articleRepository.SaveChangesAsync();
        return mapper.Map<ArticleDto>(article);
    }

    public async Task DeleteAsync(int id, int userId, Role role)
    {
        var article = await articleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Article {id} not found.");

        if (article.AuthorId != userId && role != Role.Admin)
            throw new ForbiddenException();

        await articleRepository.DeleteAsync(article);
        await articleRepository.SaveChangesAsync();
    }
}
