using Microsoft.AspNetCore.Authentication.Cookies;
using MiESCOM.Api.Auth;

var builder = WebApplication.CreateBuilder(args);

// La contraseña del usuario de demostración es pública en el repo, así que la
// aplicación se niega a arrancar fuera de desarrollo hasta que se reemplace.
if (!builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "MiESCOM solo tiene un login de demostración. Reemplaza DemoCredentialValidator antes de publicar.");
}

builder.Services.AddSingleton<ICredentialValidator, DemoCredentialValidator>();

var sessionSettings = SessionSettings.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(sessionSettings);

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
        // La sesión caduca tras el tiempo de inactividad configurado. Cada petición con
        // sesión válida reinicia el plazo completo (por defecto solo se renueva cuando
        // ya pasó la mitad, lo que acortaría el tiempo real de inactividad permitido).
        options.ExpireTimeSpan = sessionSettings.IdleTimeout;
        options.SlidingExpiration = true;
        options.Events.OnValidatePrincipal = context =>
        {
            context.ShouldRenew = true;
            return Task.CompletedTask;
        };

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
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();

app.Run();
