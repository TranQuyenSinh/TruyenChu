using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using truyenchu.Models;

namespace truyenchu.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [Route("{storySlug}")]
    public IActionResult DetailStory([FromRoute] string storySlug)
    {
        return View();
    }

    [Route("{storySlug}/chuong-{chapter:int}")]
    public IActionResult Chapter([FromRoute] string storySlug, [FromRoute] int chapter)
    {
        return View();
    }

    [Route("tim-kiem/{searchString}")]
    [HttpPost]
    public IActionResult SearchStory(string searchString) {
        return View((object)searchString);
    }

    [Route("test.html")]
    public IActionResult Test()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
