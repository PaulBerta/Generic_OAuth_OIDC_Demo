using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using OAuthTraining.Data;
using OAuthTraining.Models;
using System.Threading.Tasks;

namespace OAuthTraining.Controllers
{
    public class AccountController : Controller
    {
        private readonly IdpConfigRepository _repository;
        private readonly IDataProtector _protector;

        public AccountController(IdpConfigRepository repository, IDataProtectionProvider provider)
        {
            _repository = repository;
            _protector = provider.CreateProtector("IdpConfig.ClientSecret");
        }

        [HttpGet]
        public IActionResult Authenticate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Authenticate(IdpConfig model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var encryptedSecret = _protector.Protect(model.ClientSecret);
            var config = new IdpConfig
            {
                Authority = model.Authority,
                TenantId = model.TenantId,
                ClientId = model.ClientId,
                ClientSecret = encryptedSecret
            };
            await _repository.AddAsync(config);
            return RedirectToAction("Authenticate"); // Or redirect elsewhere
        }
    }
}
