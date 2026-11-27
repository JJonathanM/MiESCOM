using System.Security.Cryptography;

namespace MiESCOM.Api.Auth;

/// <summary>
/// Reglas de la contraseña temporal y de la nueva contraseña.
/// Las reglas definitivas están pendientes de confirmar con el profesor.
/// </summary>
public static class PasswordRules
{
    public const int MinLength = 8;
    public const int MaxLength = 128;
    public const int TemporaryLength = 12;

    // Sin caracteres que se confunden al leerlos: 0/O, 1/l/I.
    private const string TemporaryAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";

    /// <summary>Contraseña temporal de 12 caracteres, con al menos una mayúscula, una minúscula y un número.</summary>
    public static string GenerateTemporary()
    {
        while (true)
        {
            var candidate = new string(RandomNumberGenerator.GetItems<char>(TemporaryAlphabet, TemporaryLength));
            if (candidate.Any(char.IsAsciiLetterUpper)
                && candidate.Any(char.IsAsciiLetterLower)
                && candidate.Any(char.IsAsciiDigit))
            {
                return candidate;
            }
        }
    }

    /// <returns>El mensaje de error, o null si la nueva contraseña es válida.</returns>
    public static string? Validate(string newPassword, string boleta)
    {
        if (newPassword.Length < MinLength)
        {
            return $"La nueva contraseña debe tener al menos {MinLength} caracteres.";
        }
        if (newPassword.Length > MaxLength)
        {
            return $"La nueva contraseña no puede tener más de {MaxLength} caracteres.";
        }
        if (newPassword.Contains(boleta, StringComparison.Ordinal))
        {
            return "La nueva contraseña no puede contener tu boleta.";
        }
        return null;
    }
}
