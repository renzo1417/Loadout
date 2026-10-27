using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using MidtermExam.Components;
using MidtermExam.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/login";
    options.Cookie.Name = "Loadout.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Events.OnValidatePrincipal = async context =>
    {
        var store = context.HttpContext.RequestServices.GetRequiredService<AccountStore>();
        if (store.FindSession(context.Principal?.FindFirst("session_id")?.Value) == null)
        {
            context.RejectPrincipal();
            await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync(context.HttpContext);
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddSingleton<AccountStore>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILoadoutService, LoadoutService>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapPost("/auth/login", AuthEndpoints.Login);
app.MapPost("/auth/register", AuthEndpoints.Register);
app.MapPost("/auth/logout", AuthEndpoints.Logout);
app.MapPost("/auth/demo", AuthEndpoints.Demo);
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
