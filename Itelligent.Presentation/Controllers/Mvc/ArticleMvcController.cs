using Microsoft.AspNetCore.Mvc;

namespace Itelligent.Presentation.Controllers.Mvc;

[Route("article")]
public class ArticleMvcController : Controller
{
    [HttpGet("manage")]
    public IActionResult Manage() => View();

    [HttpGet("detail/{id:int}")]
    public IActionResult Detail(int id)
    {
        ViewBag.ArticleId = id;
        return View();
    }
}
