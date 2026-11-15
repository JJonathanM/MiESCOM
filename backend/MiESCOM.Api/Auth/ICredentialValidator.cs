namespace MiESCOM.Api.Auth;

public sealed record AppUser(string Id, string Boleta, string DisplayName);

/// <summary>
/// Punto de cambio del login: cuando exista la base de datos, basta con otra implementación.
/// </summary>
public interface ICredentialValidator
{
    /// <returns>El usuario si las credenciales son correctas; de lo contrario, null.</returns>
    Task<AppUser?> ValidateAsync(string boleta, string password, CancellationToken cancellationToken);
}
