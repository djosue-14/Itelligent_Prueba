using AutoMapper;
using Itelligent.Application.DTOs.Articles;
using Itelligent.Application.Exceptions;
using Itelligent.Application.Mappings;
using Itelligent.Application.Services;
using Itelligent.Domain.Entities;
using Itelligent.Domain.Enums;
using Itelligent.Domain.Interfaces;
using Moq;

namespace Itelligent.Tests.Application;

public class ArticleServiceTests
{
    private readonly Mock<IArticleRepository> _repoMock = new();
    private readonly IMapper _mapper;
    private readonly ArticleService _service;

    public ArticleServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _service = new ArticleService(_repoMock.Object, _mapper);
    }

    [Fact]
    public async Task Create_ShouldPersistArticle()
    {
        // Arrange
        var dto = new CreateArticleDto
        {
            Title = "Test Title",
            Summary = "Test Summary here",
            Content = "Test Content here long enough"
        };

        Article? saved = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Article>()))
            .Callback<Article>(a => { saved = a; a.Id = 1; a.Author = new User { Username = "author" }; })
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(() => saved);

        // Act
        var result = await _service.CreateAsync(dto, authorId: 1);

        // Assert
        Assert.Equal("Test Title", result.Title);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Article>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_ByNonAuthorNonAdmin_ShouldThrowForbidden()
    {
        // Arrange
        var article = new Article
        {
            Id = 1,
            Title = "Original",
            Summary = "Summary here",
            Content = "Content here",
            AuthorId = 10,
            Author = new User { Username = "original_author" }
        };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(article);

        var dto = new UpdateArticleDto { Title = "Changed", Summary = "Changed summary", Content = "Changed content" };

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.UpdateAsync(1, dto, userId: 99, role: Role.User));
    }

    [Fact]
    public async Task Update_ByAuthor_ShouldSucceed()
    {
        // Arrange
        var article = new Article
        {
            Id = 1,
            Title = "Original",
            Summary = "Summary here",
            Content = "Content here",
            AuthorId = 5,
            Author = new User { Username = "author" }
        };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(article);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new UpdateArticleDto { Title = "Updated", Summary = "Updated summary", Content = "Updated content" };

        // Act
        var result = await _service.UpdateAsync(1, dto, userId: 5, role: Role.User);

        // Assert
        Assert.Equal("Updated", result.Title);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_ByAdmin_ShouldSucceed()
    {
        // Arrange
        var article = new Article
        {
            Id = 2,
            Title = "Article",
            Summary = "Summary",
            Content = "Content",
            AuthorId = 99,
            Author = new User { Username = "someone" }
        };
        _repoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(article);
        _repoMock.Setup(r => r.DeleteAsync(article)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act — admin deletes another user's article
        await _service.DeleteAsync(2, userId: 1, role: Role.Admin);

        // Assert
        _repoMock.Verify(r => r.DeleteAsync(article), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_ByNonAuthorUser_ShouldThrowForbidden()
    {
        // Arrange
        var article = new Article { Id = 3, AuthorId = 10, Author = new User { Username = "owner" } };
        _repoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(article);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.DeleteAsync(3, userId: 99, role: Role.User));
    }
}
