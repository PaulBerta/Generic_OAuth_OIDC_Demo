using Microsoft.AspNetCore.Mvc;

namespace OAuthTraining.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Authenticate()
        {
            return View();
        }
    }
}
