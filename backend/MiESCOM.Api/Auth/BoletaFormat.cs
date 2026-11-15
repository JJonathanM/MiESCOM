using System.Diagnostics.CodeAnalysis;

namespace MiESCOM.Api.Auth;

/// <summary>
/// Por ahora solo se aceptan boletas de 10 dígitos. Falta definir cómo se manejarán
/// las boletas provisionales (pre-boletas), que incluyen letras.
/// </summary>
public static class BoletaFormat
{
    public const int Length = 10;

    public static bool IsValid([NotNullWhen(true)] string? value) =>
        value is { Length: Length } && value.All(char.IsAsciiDigit);
}
