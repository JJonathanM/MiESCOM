using Microsoft.AspNetCore.Identity;

namespace MiESCOM.Api.Auth;

/// <summary>
/// Un único usuario ficticio para probar el concepto sin base de datos.
/// Program.cs impide que se ejecute fuera de desarrollo.
/// </summary>
public sealed class DemoCredentialValidator : ICredentialValidator
{
    private const string DemoUsername = "alumno.demo";
    private const string DemoPassword = "Demo1234!";

    private static readonly AppUser DemoUser = new("demo-1", DemoUsername, "Alumno de Prueba");

    private readonly PasswordHasher<AppUser> hasher = new();
    private readonly string demoPasswordHash;

    public DemoCredentialValidator()
    {
        demoPasswordHash = hasher.HashPassword(DemoUser, DemoPassword);
    }

    public Task<AppUser?> ValidateAsync(string username, string password, CancellationToken cancellationToken)
    {
        var isDemoUser = string.Equals(username, DemoUsername, StringComparison.OrdinalIgnoreCase);

        // Siempre se verifica la contraseña para que el tiempo de respuesta no revele si el usuario existe.
        var passwordMatches = hasher.VerifyHashedPassword(DemoUser, demoPasswordHash, password)
            != PasswordVerificationResult.Failed;

        return Task.FromResult(isDemoUser && passwordMatches ? DemoUser : null);
    }
}
