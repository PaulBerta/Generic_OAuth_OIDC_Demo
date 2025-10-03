using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using OAuthTraining.Data;
using OAuthTraining.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OAuthTraining.Controllers
{
    public class AccountController : Controller
    {
        private readonly IdpConfigRepository _repository;
        private readonly IDataProtector _protector;
        private readonly IOptionsMonitorCache<OpenIdConnectOptions> _optionsCache;

        public AccountController(
            IdpConfigRepository repository,
            IDataProtectionProvider provider,
            IOptionsMonitorCache<OpenIdConnectOptions> optionsCache)
        {
            _repository = repository;
            _protector = provider.CreateProtector("IdpConfig.ClientSecret");
            _optionsCache = optionsCache;
        }

        [HttpGet]
        public async Task<IActionResult> Authenticate()
        {
            if (await _repository.HasAnyAsync())
            {
                return RedirectToAction("Index", "Home");
            }

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

            _optionsCache.TryRemove(OpenIdConnectDefaults.AuthenticationScheme);

            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Index", "Home")
                },
                OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
