using Microsoft.AspNetCore.Mvc;

namespace Itelligent.Presentation.Controllers.Mvc;

[Route("auth")]
public class AuthMvcController : Controller
{
    [HttpGet("login")]
    public IActionResult Login() => View();

}
