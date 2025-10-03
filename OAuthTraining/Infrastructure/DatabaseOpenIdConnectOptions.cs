using System;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OAuthTraining.Data;

namespace OAuthTraining.Infrastructure
{
    /// <summary>
    /// Populates <see cref="OpenIdConnectOptions"/> instances from the database at runtime so the
    /// application can evolve its identity provider settings without a redeploy.
    /// </summary>
    public class DatabaseOpenIdConnectOptions : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DatabaseOpenIdConnectOptions(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void Configure(string? name, OpenIdConnectOptions options)
        {
            // Only handle the default OpenIdConnect scheme; other named schemes should fall back to
            // the framework's default behavior.
            if (!string.Equals(name, OpenIdConnectDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IdpConfigRepository>();
            var config = repository.GetCurrentAsync().GetAwaiter().GetResult();

            if (config is null)
            {
                // With no data present yet we seed obvious placeholders.  The UI uses these values
                // for the very first challenge before a real configuration has been collected.
                options.Authority = "https://placeholder.invalid";
                options.ClientId = "placeholder";
                options.ClientSecret = "placeholder";
                return;
            }

            var dataProtectionProvider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProtectionProvider.CreateProtector("IdpConfig.ClientSecret");
            var clientSecret = protector.Unprotect(config.ClientSecret);

            // Apply the values that were supplied through the AccountController onboarding flow.
            options.Authority = config.Authority;
            options.ClientId = config.ClientId;
            options.ClientSecret = clientSecret;
        }

        public void Configure(OpenIdConnectOptions options)
        {
            Configure(OpenIdConnectDefaults.AuthenticationScheme, options);
        }
    }
}
