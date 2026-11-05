namespace MiESCOM.Api.Auth;

public sealed record AppUser(string Id, string Username, string DisplayName);

/// <summary>
/// Punto de cambio del login: cuando se defina de dónde salen los usuarios
/// (base de datos, sistema de la escuela, etc.), basta con otra implementación.
/// </summary>
public interface ICredentialValidator
{
    /// <returns>El usuario si las credenciales son correctas; de lo contrario, null.</returns>
    Task<AppUser?> ValidateAsync(string username, string password, CancellationToken cancellationToken);
}
