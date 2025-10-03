using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OAuthTraining.Data;
using OAuthTraining.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;

        options.RequireHttpsMetadata = false;
    });

builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, DatabaseOpenIdConnectOptions>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var defaultRoute = hasExistingConfig
    ? new { controller = "Home", action = "Index" }
    : new { controller = "Account", action = "Authenticate" };

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}",
    defaults: defaultRoute);

app.Run();
