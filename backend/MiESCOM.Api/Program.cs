using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiESCOM.Api.Alumnos;
using MiESCOM.Api.Auth;
using MiESCOM.Api.Data;
using MiESCOM.Api.Email;

var builder = WebApplication.CreateBuilder(args);

// Base de datos MySQL. La cadena de conexión no va en el repo: en desarrollo se guarda con
// "dotnet user-secrets" (ver la guía rápida).
var connectionString = builder.Configuration.GetConnectionString(MiEscomDbContext.ConnectionStringName);
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        $"Falta la cadena de conexión ConnectionStrings:{MiEscomDbContext.ConnectionStringName}. " +
        "En desarrollo se guarda con dotnet user-secrets (ver la guía rápida).");
}

builder.Services.AddDbContext<MiEscomDbContext>(options => options
    .UseMySQL(connectionString)
    .UseSnakeCaseNamingConvention());

var sessionSettings = SessionSettings.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(sessionSettings);
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton(EmailSettings.FromConfiguration(builder.Configuration, builder.Environment));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddProblemDetails();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "miescom.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        // La sesión caduca tras el tiempo de inactividad configurado (Session:IdleTimeoutMinutes).
        options.ExpireTimeSpan = sessionSettings.IdleTimeout;
        options.SlidingExpiration = true;
        options.Events.OnValidatePrincipal = SessionValidation.ValidateAsync;

        // Es una API: responde con códigos de estado en lugar de redirigir a una página de login.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthClaims.PasswordChangedPolicy, policy => policy
        .RequireAuthenticatedUser()
        .RequireAssertion(context => !context.User.MustChange()));

// Límite de intentos por dirección de red, porque las boletas son fáciles de adivinar.
var loginPerMinute = builder.Configuration.GetValue("RateLimits:LoginPerMinute", 10);
var registroPer15Minutes = builder.Configuration.GetValue("RateLimits:RegistroPer15Minutes", 5);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, cancellationToken) =>
        new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Detail = "Hiciste demasiados intentos. Espera unos minutos y vuelve a intentarlo.",
            },
            cancellationToken));

    options.AddPolicy(AuthEndpoints.LoginRateLimit, httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = loginPerMinute, Window = TimeSpan.FromMinutes(1) }));

    options.AddPolicy(AuthEndpoints.RegistroRateLimit, httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = registroPer15Minutes, Window = TimeSpan.FromMinutes(15) }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await InicializadorBaseDeDatos.PrepararParaDesarrolloAsync(app);
}
else
{
    // Fuera de desarrollo, los errores inesperados responden 500 sin detalles internos.
    app.UseExceptionHandler();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapAuthEndpoints();
app.MapAlumnoEndpoints();

app.Run();
