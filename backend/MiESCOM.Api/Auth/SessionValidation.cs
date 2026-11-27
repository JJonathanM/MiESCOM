using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MiESCOM.Api.Data;

namespace MiESCOM.Api.Auth;

/// <summary>Se ejecuta en cada petición que trae la cookie de sesión.</summary>
public static class SessionValidation
{
    public static async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        // Cada petición con sesión válida reinicia el plazo completo de inactividad (por defecto
        // solo se renueva cuando ya pasó la mitad, lo que acortaría el tiempo real permitido).
        context.ShouldRenew = true;

        var boleta = context.Principal?.FindFirstValue(ClaimTypes.Name);
        var sello = context.Principal?.FindFirstValue(AuthClaims.SecurityStamp);

        var vigente = false;
        if (boleta is not null && sello is not null)
        {
            // Si cambió la contraseña (nuevo sello) o se desactivó la cuenta, la sesión deja de valer.
            var db = context.HttpContext.RequestServices.GetRequiredService<MiEscomDbContext>();
            vigente = await db.Usuarios.AsNoTracking().AnyAsync(
                u => u.Boleta == boleta && u.Activo && u.SelloSeguridad == sello,
                context.HttpContext.RequestAborted);
        }

        if (!vigente)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
