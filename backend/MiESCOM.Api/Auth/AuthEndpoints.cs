using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MiESCOM.Api.Auth;

public static class AuthEndpoints
{
    private const string DisplayNameClaim = "display_name";

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("/login", Login);
        auth.MapPost("/logout", Logout);
        auth.MapGet("/me", GetCurrentUser).RequireAuthorization();
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        ICredentialValidator credentials,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var user = await credentials.ValidateAsync(
            request.Username ?? string.Empty,
            request.Password ?? string.Empty,
            cancellationToken);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(DisplayNameClaim, user.DisplayName),
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Results.Ok(new UserResponse(user.Username, user.DisplayName));
    }

    private static async Task Logout(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
    }

    private static IResult GetCurrentUser(ClaimsPrincipal user) =>
        Results.Ok(new UserResponse(
            user.FindFirstValue(ClaimTypes.Name)!,
            user.FindFirstValue(DisplayNameClaim)!));

    private sealed record LoginRequest(string? Username, string? Password);

    private sealed record UserResponse(string Username, string DisplayName);
}
