using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using MiESCOM.Api.Data;

namespace MiESCOM.Api.Auth;

/// <summary>Datos que viajan en la cookie de sesión.</summary>
public static class AuthClaims
{
    public const string DisplayName = "display_name";
    public const string SecurityStamp = "sello";
    public const string MustChangePassword = "debe_cambiar_password";

    /// <summary>Política para las secciones que exigen haber cambiado la contraseña temporal.</summary>
    public const string PasswordChangedPolicy = "PasswordDefinitiva";

    public static ClaimsPrincipal CreatePrincipal(Usuario usuario, Alumno alumno)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, usuario.Boleta),
                new Claim(ClaimTypes.Name, usuario.Boleta),
                new Claim(DisplayName, DisplayNameOf(alumno)),
                new Claim(SecurityStamp, usuario.SelloSeguridad),
                new Claim(MustChangePassword, usuario.PasswordTemporal ? "true" : "false"),
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }

    public static string DisplayNameOf(Alumno alumno) => $"{alumno.Nombre} {alumno.PrimerApellido}";

    public static string Boleta(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Name)!;

    public static bool MustChange(this ClaimsPrincipal user) =>
        user.FindFirstValue(MustChangePassword) == "true";

    public static string NewSecurityStamp() => Guid.NewGuid().ToString("N");
}
