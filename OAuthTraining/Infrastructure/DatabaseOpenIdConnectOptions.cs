using System;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OAuthTraining.Data;

namespace OAuthTraining.Infrastructure
{
    public class DatabaseOpenIdConnectOptions : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DatabaseOpenIdConnectOptions(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void Configure(string? name, OpenIdConnectOptions options)
        {
            if (!string.Equals(name, OpenIdConnectDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IdpConfigRepository>();
            var config = repository.GetCurrentAsync().GetAwaiter().GetResult();

            if (config is null)
            {
                return;
            }

            var dataProtectionProvider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProtectionProvider.CreateProtector("IdpConfig.ClientSecret");
            var clientSecret = protector.Unprotect(config.ClientSecret);

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
