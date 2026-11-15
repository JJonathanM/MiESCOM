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
        if (!BoletaFormat.IsValid(request.Boleta))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["boleta"] = [$"La boleta debe tener {BoletaFormat.Length} dígitos."],
            });
        }

        var user = await credentials.ValidateAsync(
            request.Boleta,
            request.Password ?? string.Empty,
            cancellationToken);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Boleta),
                new Claim(DisplayNameClaim, user.DisplayName),
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Results.Ok(new UserResponse(user.Boleta, user.DisplayName));
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

    private sealed record LoginRequest(string? Boleta, string? Password);

    private sealed record UserResponse(string Boleta, string DisplayName);
}
