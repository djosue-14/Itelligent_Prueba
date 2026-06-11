using Microsoft.AspNetCore.Mvc;

namespace Itelligent.Presentation.Controllers.Mvc;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Error() => View();
}
