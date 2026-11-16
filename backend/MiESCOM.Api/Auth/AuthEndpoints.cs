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
        SessionSettings session,
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

        return Results.Ok(new SessionResponse(user.Boleta, user.DisplayName, session));
    }

    private static async Task Logout(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
    }

    // Cada llamada con sesión válida también reinicia el plazo de inactividad (ver Program.cs).
    private static IResult GetCurrentUser(ClaimsPrincipal user, SessionSettings session) =>
        Results.Ok(new SessionResponse(
            user.FindFirstValue(ClaimTypes.Name)!,
            user.FindFirstValue(DisplayNameClaim)!,
            session));

    private sealed record LoginRequest(string? Boleta, string? Password);

    private sealed record SessionResponse(string Boleta, string DisplayName, int IdleTimeoutSeconds)
    {
        public SessionResponse(string boleta, string displayName, SessionSettings session)
            : this(boleta, displayName, (int)session.IdleTimeout.TotalSeconds)
        {
        }
    }
}
