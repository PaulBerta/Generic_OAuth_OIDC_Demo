using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OAuthTraining.Models;

namespace OAuthTraining.Controllers;

/// <summary>
/// Simple MVC controller whose actions require authentication.  Once the identity provider
/// settings have been saved the AccountController redirects here, which in turn forces the
/// OpenID Connect challenge when the user is not yet signed in.
/// </summary>
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // At this point the user is authenticated; render the landing page.
        return View();
    }

    public IActionResult Privacy()
    {
        // Render a secondary view that is also protected by the [Authorize] attribute.
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Surface the request identifier to help with diagnostics in logs and telemetry.
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}