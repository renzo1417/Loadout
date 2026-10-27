using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.WebUtilities;
using MidtermExam.Models;
namespace MidtermExam.Services;

public static class AuthEndpoints
{
    public static async Task<IResult> Login(HttpContext context, AccountStore store, IAntiforgery antiforgery)
    {
        if (!await ValidateRequest(context, antiforgery)) return Results.BadRequest("Refresh the page and try again.");
        var form = await context.Request.ReadFormAsync();
        var model = new LoginModel { Email = form["Email"].ToString().Trim(), Password = form["Password"].ToString() };
        var error = Validate(model);
        if (error != null) return Failure("/login", error, model.Email);
        var account = store.Verify(model.Email, model.Password);
        if (account == null) return Failure("/login", "credentials", model.Email);
        await SignIn(context, store, account);
        return Results.LocalRedirect("/dashboard");
    }
    public static async Task<IResult> Register(HttpContext context, AccountStore store, IAntiforgery antiforgery)
    {
        if (!await ValidateRequest(context, antiforgery)) return Results.BadRequest("Refresh the page and try again.");
        var form = await context.Request.ReadFormAsync();
        var model = new RegisterModel { DisplayName = form["DisplayName"].ToString().Trim(), Email = form["Email"].ToString().Trim(),
            Password = form["Password"].ToString(), ConfirmPassword = form["ConfirmPassword"].ToString() };
        var error = Validate(model);
        if (error != null) return Failure("/register", error, model.Email, model.DisplayName);
        var account = store.Register(model);
        if (account == null) return Failure("/register", "duplicate", model.Email, model.DisplayName);
        await SignIn(context, store, account);
        return Results.LocalRedirect("/dashboard");
    }
    public static async Task<IResult> Logout(HttpContext context, AccountStore store, IAntiforgery antiforgery)
    {
        if (!await ValidateRequest(context, antiforgery)) return Results.BadRequest("Refresh the page and try again.");
        store.EndSession(context.User.FindFirst("session_id")?.Value);
        await context.SignOutAsync();
        return Results.LocalRedirect("/login?notice=loggedout");
    }
    public static async Task<IResult> Demo(HttpContext context, AccountStore store, IAntiforgery antiforgery)
    {
        if (!await ValidateRequest(context, antiforgery)) return Results.BadRequest("Refresh the page and try again.");
        // Never replace the identity or loadout of an already signed-in account.
        if (context.User.Identity?.IsAuthenticated != true) await SignIn(context, store, store.CreateDemo());
        return Results.LocalRedirect("/dashboard");
    }
    private static async Task SignIn(HttpContext context, AccountStore store, UserAccount account)
    {
        store.EndSession(context.User.FindFirst("session_id")?.Value);
        var identity = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, account.Profile.Id.ToString()),
            new Claim(ClaimTypes.Name, account.Profile.DisplayName),
            new Claim("session_id", store.StartSession(account))
        }, CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = false });
    }
    private static string? Validate(object model)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), errors, true);
        return errors.FirstOrDefault()?.MemberNames.FirstOrDefault();
    }
    private static IResult Failure(string path, string error, string email, string? name = null) =>
        Results.LocalRedirect(QueryHelpers.AddQueryString(path, new Dictionary<string,string?> {
            ["error"] = error, ["email"] = email[..Math.Min(email.Length, 254)], ["name"] = name == null ? null : name[..Math.Min(name.Length,30)] }));
    private static async Task<bool> ValidateRequest(HttpContext context, IAntiforgery antiforgery)
    {
        try { await antiforgery.ValidateRequestAsync(context); return true; }
        catch (AntiforgeryValidationException) { return false; }
    }
}
