using System.Globalization;

namespace MiESCOM.Api.Alumnos;

/// <summary>
/// Obtiene la fecha de nacimiento y el sexo a partir de la CURP, para no guardarlos aparte.
/// Estructura: AAAA AAMMDD S EE CCC X D, donde S es el sexo (H, M o X) y X es un dígito
/// para quienes nacieron antes de 2000 y una letra para quienes nacieron en 2000 o después.
/// </summary>
public static class Curp
{
    public static DateOnly? FechaNacimiento(string? curp)
    {
        if (curp is not { Length: 18 })
        {
            return null;
        }

        var siglo = char.IsAsciiDigit(curp[16]) ? 1900 : 2000;
        if (!int.TryParse(curp.AsSpan(4, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var anio)
            || !int.TryParse(curp.AsSpan(6, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var mes)
            || !int.TryParse(curp.AsSpan(8, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var dia))
        {
            return null;
        }

        try
        {
            return new DateOnly(siglo + anio, mes, dia);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    public static string? Sexo(string? curp) => curp is { Length: 18 }
        ? char.ToUpperInvariant(curp[10]) switch
        {
            'H' => "Hombre",
            'M' => "Mujer",
            'X' => "No binario",
            _ => null,
        }
        : null;
}
