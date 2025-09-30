using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OAuthTraining.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        var oidcConfig = builder.Configuration.GetSection("OpenIDConnectSettings");

        options.Authority = oidcConfig["Authority"];
        options.ClientId = oidcConfig["ClientId"];
        options.ClientSecret = oidcConfig["ClientSecret"];

        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        
        options.RequireHttpsMetadata = false;
    });

var connectionString = builder.Configuration.GetConnectionString("ConnectionStrings:DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Authenticate}/{id?}");

app.Run();