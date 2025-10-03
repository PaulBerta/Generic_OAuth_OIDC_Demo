using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace OAuthTraining.Controllers;

public class AdminController : Controller
{
    private readonly IHostApplicationLifetime _lifetime;

    public AdminController(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    public IActionResult Restart()
    {
        HttpContext.Response.OnCompleted(() =>
        {
            _lifetime.StopApplication();
            return Task.CompletedTask;
        });

        return RedirectToAction("Index", "Home");
    }
}