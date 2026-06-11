using System.Security.Claims;
using Itelligent.Application.DTOs.Articles;
using Itelligent.Application.DTOs.Comments;
using Itelligent.Application.Services;
using Itelligent.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Itelligent.Presentation.Controllers.Api;

[ApiController]
[Route("api/articles")]
public class ArticlesController(ArticleService articleService, CommentService commentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await articleService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await articleService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateArticleDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var authorId = GetUserId();
        var result = await articleService.CreateAsync(dto, authorId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateArticleDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await articleService.UpdateAsync(id, dto, GetUserId(), GetUserRole());
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        await articleService.DeleteAsync(id, GetUserId(), GetUserRole());
        return NoContent();
    }

    [HttpPost("{id:int}/comments")]
    [Authorize]
    public async Task<IActionResult> AddComment(int id, [FromBody] CreateCommentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await commentService.AddCommentAsync(id, GetUserId(), dto);
        return Ok(result);
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);

    private Role GetUserRole()
    {
        var roleStr = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        return Enum.Parse<Role>(roleStr);
    }
}
