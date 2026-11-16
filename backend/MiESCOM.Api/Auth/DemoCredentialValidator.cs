using Microsoft.AspNetCore.Identity;

namespace MiESCOM.Api.Auth;

/// <summary>
/// Un único usuario ficticio para probar el concepto sin base de datos.
/// Program.cs impide que se ejecute fuera de desarrollo.
/// </summary>
public sealed class DemoCredentialValidator : ICredentialValidator
{
    // Año 2099: no puede coincidir con la boleta de un alumno real.
    private const string DemoBoleta = "2099630001";
    private const string DemoPassword = "Demo1234!";

    private static readonly AppUser DemoUser = new("demo-1", DemoBoleta, "Alumno de Prueba");

    private readonly PasswordHasher<AppUser> hasher = new();
    private readonly string demoPasswordHash;

    public DemoCredentialValidator()
    {
        demoPasswordHash = hasher.HashPassword(DemoUser, DemoPassword);
    }

    public Task<AppUser?> ValidateAsync(string boleta, string password, CancellationToken cancellationToken)
    {
        var isDemoUser = string.Equals(boleta, DemoBoleta, StringComparison.Ordinal);

        // Siempre se verifica la contraseña para que el tiempo de respuesta no revele si la boleta existe.
        var passwordMatches = hasher.VerifyHashedPassword(DemoUser, demoPasswordHash, password)
            != PasswordVerificationResult.Failed;

        return Task.FromResult(isDemoUser && passwordMatches ? DemoUser : null);
    }
}
