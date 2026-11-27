using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MiESCOM.Api.Data;
using MiESCOM.Api.Email;

namespace MiESCOM.Api.Auth;

public static class AuthEndpoints
{
    public const string LoginRateLimit = "login";
    public const string RegistroRateLimit = "registro";

    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("/login", Login).RequireRateLimiting(LoginRateLimit);
        auth.MapPost("/logout", Logout);
        auth.MapGet("/me", GetCurrentUser).RequireAuthorization();
        auth.MapPost("/registro", Registro).RequireRateLimiting(RegistroRateLimit);
        auth.MapPost("/cambiar-password", CambiarPassword)
            .RequireAuthorization()
            .RequireRateLimiting(LoginRateLimit);
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        MiEscomDbContext db,
        PasswordHasher hasher,
        SessionSettings session,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!BoletaFormat.IsValid(request.Boleta))
        {
            return InvalidBoleta();
        }

        var password = request.Password ?? string.Empty;
        var usuario = await db.Usuarios
            .Include(u => u.Alumno)
            .FirstOrDefaultAsync(u => u.Boleta == request.Boleta, cancellationToken);

        if (usuario is null || !usuario.Activo)
        {
            hasher.SimulateVerify(password);
            return Results.Unauthorized();
        }

        var now = DateTime.UtcNow;
        if (usuario.BloqueadoHasta > now)
        {
            return Locked(usuario.BloqueadoHasta.Value - now);
        }

        if (!hasher.Verify(password, usuario.PasswordHash, out var needsRehash))
        {
            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= MaxFailedAttempts)
            {
                usuario.IntentosFallidos = 0;
                usuario.BloqueadoHasta = now + LockoutDuration;
                await db.SaveChangesAsync(cancellationToken);
                return Locked(LockoutDuration);
            }

            await db.SaveChangesAsync(cancellationToken);
            return Results.Unauthorized();
        }

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        usuario.UltimoAcceso = now;
        if (needsRehash)
        {
            usuario.PasswordHash = hasher.Hash(password);
        }
        await db.SaveChangesAsync(cancellationToken);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            AuthClaims.CreatePrincipal(usuario, usuario.Alumno));

        return Results.Ok(SessionResponse.For(usuario, usuario.Alumno, session));
    }

    private static async Task Logout(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
    }

    // Cada llamada con sesión válida también reinicia el plazo de inactividad (ver SessionValidation).
    private static IResult GetCurrentUser(ClaimsPrincipal user, SessionSettings session) =>
        Results.Ok(new SessionResponse(
            user.Boleta(),
            user.FindFirstValue(AuthClaims.DisplayName)!,
            user.MustChange(),
            (int)session.IdleTimeout.TotalSeconds));

    /// <summary>
    /// Crea la cuenta de un alumno que existe en la tabla de alumnos y le envía por correo
    /// una contraseña temporal. Al entrar con ella se le pide cambiarla.
    /// </summary>
    private static async Task<IResult> Registro(
        RegistroRequest request,
        MiEscomDbContext db,
        PasswordHasher hasher,
        IEmailSender emailSender,
        EmailSettings emailSettings,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        if (!BoletaFormat.IsValid(request.Boleta))
        {
            return InvalidBoleta();
        }

        var alumno = await db.Alumnos
            .Include(a => a.Usuario)
            .FirstOrDefaultAsync(a => a.Boleta == request.Boleta, cancellationToken);

        if (alumno is null)
        {
            return Problem(
                StatusCodes.Status404NotFound,
                "No encontramos esa boleta en el registro de alumnos de ESCOM. Verifica que esté bien escrita.");
        }
        if (alumno.Usuario is not null)
        {
            return AlreadyRegistered();
        }

        var correo = (alumno.EmailInstitucional ?? alumno.EmailPersonal)?.Trim();
        if (string.IsNullOrEmpty(correo))
        {
            return Problem(
                StatusCodes.Status422UnprocessableEntity,
                "No tienes un correo registrado para enviarte la contraseña. Acude a Gestión Escolar para actualizar tus datos.");
        }

        var temporal = PasswordRules.GenerateTemporary();
        db.Usuarios.Add(new Usuario
        {
            Boleta = alumno.Boleta,
            PasswordHash = hasher.Hash(temporal),
            PasswordTemporal = true,
            SelloSeguridad = AuthClaims.NewSecurityStamp(),
            Activo = true,
        });

        // La cuenta solo queda guardada si el correo se envió; si falla, el alumno puede reintentar.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Dos solicitudes al mismo tiempo con la misma boleta.
            return AlreadyRegistered();
        }

        try
        {
            var mensaje = CorreoContrasenaTemporal.Crear(
                correo, alumno.Nombre, alumno.Boleta, temporal, emailSettings.LoginUrl);
            await emailSender.SendAsync(mensaje, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            loggerFactory.CreateLogger("MiESCOM.Registro")
                .LogError(ex, "No se pudo enviar el correo de registro de la boleta {Boleta}.", alumno.Boleta);
            return Problem(
                StatusCodes.Status503ServiceUnavailable,
                "No se pudo enviar el correo con tu contraseña. Intenta de nuevo más tarde.");
        }

        await transaction.CommitAsync(cancellationToken);
        return Results.Ok(new RegistroResponse(CorreoContrasenaTemporal.Enmascarar(correo)));
    }

    private static async Task<IResult> CambiarPassword(
        CambiarPasswordRequest request,
        ClaimsPrincipal user,
        MiEscomDbContext db,
        PasswordHasher hasher,
        SessionSettings session,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var boleta = user.Boleta();
        var usuario = await db.Usuarios
            .Include(u => u.Alumno)
            .FirstOrDefaultAsync(u => u.Boleta == boleta, cancellationToken);

        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        var actual = request.PasswordActual ?? string.Empty;
        var nueva = request.PasswordNueva ?? string.Empty;

        if (!hasher.Verify(actual, usuario.PasswordHash, out _))
        {
            return FieldProblem(
                "passwordActual",
                usuario.PasswordTemporal
                    ? "La contraseña temporal no es correcta."
                    : "La contraseña actual no es correcta.");
        }

        var error = PasswordRules.Validate(nueva, boleta);
        if (error is not null)
        {
            return FieldProblem("passwordNueva", error);
        }
        if (nueva == actual)
        {
            return FieldProblem("passwordNueva", "La nueva contraseña debe ser distinta de la actual.");
        }

        usuario.PasswordHash = hasher.Hash(nueva);
        usuario.PasswordTemporal = false;
        usuario.IntentosFallidos = 0;
        // El nuevo sello cierra las sesiones abiertas en otros dispositivos; esta se renueva abajo.
        usuario.SelloSeguridad = AuthClaims.NewSecurityStamp();
        await db.SaveChangesAsync(cancellationToken);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            AuthClaims.CreatePrincipal(usuario, usuario.Alumno));

        return Results.Ok(SessionResponse.For(usuario, usuario.Alumno, session));
    }

    private static IResult InvalidBoleta() =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["boleta"] = [$"La boleta debe tener {BoletaFormat.Length} dígitos."],
        });

    private static IResult AlreadyRegistered() =>
        Problem(StatusCodes.Status409Conflict, "Esta boleta ya tiene una cuenta. Inicia sesión con tu contraseña.");

    private static IResult Locked(TimeSpan remaining)
    {
        var minutes = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
        return Problem(
            StatusCodes.Status423Locked,
            $"Por seguridad, tu cuenta se bloqueó tras {MaxFailedAttempts} intentos fallidos. " +
            $"Intenta de nuevo en {minutes} {(minutes == 1 ? "minuto" : "minutos")}.");
    }

    private static IResult Problem(int statusCode, string detail) =>
        Results.Problem(statusCode: statusCode, detail: detail);

    private static IResult FieldProblem(string field, string detail) =>
        Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            detail: detail,
            extensions: new Dictionary<string, object?> { ["campo"] = field });

    private sealed record LoginRequest(string? Boleta, string? Password);

    private sealed record RegistroRequest(string? Boleta);

    private sealed record RegistroResponse(string Correo);

    private sealed record CambiarPasswordRequest(string? PasswordActual, string? PasswordNueva);

    private sealed record SessionResponse(
        string Boleta,
        string DisplayName,
        bool MustChangePassword,
        int IdleTimeoutSeconds)
    {
        public static SessionResponse For(Usuario usuario, Alumno alumno, SessionSettings session) =>
            new(
                usuario.Boleta,
                AuthClaims.DisplayNameOf(alumno),
                usuario.PasswordTemporal,
                (int)session.IdleTimeout.TotalSeconds);
    }
}
