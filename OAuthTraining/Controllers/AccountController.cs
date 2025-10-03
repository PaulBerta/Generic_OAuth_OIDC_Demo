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
    /// <summary>
    /// Handles the collection of identity provider settings and orchestrates the OpenID Connect
    /// challenge once a valid configuration has been saved.  The controller is responsible for
    /// clearing the cached <see cref="OpenIdConnectOptions"/> snapshot and forcing it to reload so
    /// the very next request uses the freshly persisted data.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IdpConfigRepository _idpConfigRepository;
        private readonly IDataProtector _clientSecretProtector;
        private readonly IOptionsMonitorCache<OpenIdConnectOptions> _optionsCache;
        // Used to force a new options snapshot to be materialized after clearing the cache.
        private readonly IOptionsMonitor<OpenIdConnectOptions> _optionsMonitor;

        public AccountController(
            IdpConfigRepository idpConfigRepository,
            IDataProtectionProvider dataProtectionProvider,
            IOptionsMonitorCache<OpenIdConnectOptions> optionsCache,
            IOptionsMonitor<OpenIdConnectOptions> optionsMonitor)
        {
            _idpConfigRepository = idpConfigRepository;
            _clientSecretProtector = dataProtectionProvider.CreateProtector("IdpConfig.ClientSecret");
            _optionsCache = optionsCache;
            _optionsMonitor = optionsMonitor;
        }

        /// <summary>
        /// Serves the configuration form when no identity provider settings exist yet.  Once a
        /// configuration has been stored the action immediately issues an OpenID Connect challenge
        /// using a freshly rebuilt options snapshot so the redirect reflects the new values.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Authenticate()
        {
            // If a configuration exists we immediately clear the cached OpenID Connect options and
            // warm up a fresh snapshot before issuing the challenge.  This ensures that the handler
            // used for THIS request was constructed from the latest database values instead of the
            // placeholder values seeded at application start.
            if (await _idpConfigRepository.HasAnyAsync())
            {
                _optionsCache.TryRemove(OpenIdConnectDefaults.AuthenticationScheme);
                _optionsMonitor.Get(OpenIdConnectDefaults.AuthenticationScheme);

                return Challenge(
                    new AuthenticationProperties
                    {
                        RedirectUri = Url.Action("Index", "Home")
                    },
                    OpenIdConnectDefaults.AuthenticationScheme);
            }

            // No configuration has been saved yet, so present the onboarding form to collect the
            // authority, client id, and secret from the operator.
            return View();
        }

        /// <summary>
        /// Persists the submitted identity provider configuration and redirects back to the GET
        /// action so a brand-new request can drive the OpenID Connect challenge using the updated
        /// options.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Authenticate(IdpConfig model)
        {
            if (!ModelState.IsValid)
            {
                // Validation failed, so redisplay the form and let the view surface the validation
                // messages to the operator.
                return View(model);
            }
            // Encrypt the client secret before saving so it never rests in the database as plain
            // text.  This is symmetrical with the unprotect call in DatabaseOpenIdConnectOptions.
            var encryptedSecret = _clientSecretProtector.Protect(model.ClientSecret);
            var savedConfiguration = new IdpConfig
            {
                Authority = model.Authority,
                TenantId = model.TenantId,
                ClientId = model.ClientId,
                ClientSecret = encryptedSecret
            };
            await _idpConfigRepository.AddAsync(savedConfiguration);

            // Remove the old cached snapshot.  The redirected GET action will immediately warm the
            // cache with the new data before issuing the challenge.
            _optionsCache.TryRemove(OpenIdConnectDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Authenticate));
        }
    }
}
