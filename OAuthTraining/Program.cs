using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OAuthTraining.Data;
using OAuthTraining.Infrastructure;

// The WebApplication builder wires up dependency injection, logging, configuration, etc. for the
// sample MVC application that demonstrates dynamic OpenID Connect configuration.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        // We only need the authorization code flow; all other settings are supplied dynamically by
        // DatabaseOpenIdConnectOptions once a configuration is stored.
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;

        // The training environment runs without HTTPS.  Production deployments should re-enable
        // the default requirement for HTTPS metadata.
        options.RequireHttpsMetadata = false;
    });

// DatabaseOpenIdConnectOptions reads identity provider settings from the database and pushes them
// into the OpenIdConnectOptions instance that ASP.NET Core uses to wire the handler.
builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, DatabaseOpenIdConnectOptions>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Expose the repository for controllers and infrastructure components that interact with the
// stored identity provider configuration.
builder.Services.AddScoped<IdpConfigRepository>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

var hasExistingConfig = false;
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var repository = scope.ServiceProvider.GetRequiredService<IdpConfigRepository>();
    hasExistingConfig = await repository.HasAnyAsync();
}

// Standard ASP.NET Core middleware pipeline.
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Choose the default route based on whether an identity provider configuration already exists.
var defaultRoute = hasExistingConfig
    ? new { controller = "Home", action = "Index" }
    : new { controller = "Account", action = "Authenticate" };

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}",
    defaults: defaultRoute);

app.Run();
