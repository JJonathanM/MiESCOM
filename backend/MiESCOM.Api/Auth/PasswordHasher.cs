using System.Security.Cryptography;
using System.Text;

namespace MiESCOM.Api.Auth;

/// <summary>
/// Guarda contraseñas con SHA-256 aplicado mediante PBKDF2 (HMAC-SHA256), con una sal
/// aleatoria por usuario, como recomienda OWASP. Viene incluido en .NET.
///
/// Formato guardado: <c>pbkdf2-sha256$iteraciones$sal$hash</c> (sal y hash en Base64).
/// Como cada hash guarda sus iteraciones, el valor de Passwords:Iterations se puede cambiar
/// después sin invalidar las contraseñas existentes: se actualizan al siguiente inicio de sesión.
/// </summary>
public sealed class PasswordHasher
{
    private const string Algorithm = "pbkdf2-sha256";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int DefaultIterations = 600_000;

    private readonly int iterations;
    private readonly Lazy<string> dummyHash;

    public PasswordHasher(IConfiguration configuration)
    {
        iterations = configuration.GetValue("Passwords:Iterations", DefaultIterations);
        if (iterations < 1)
        {
            throw new InvalidOperationException("Passwords:Iterations debe ser al menos 1.");
        }
        dummyHash = new Lazy<string>(() => Hash(Convert.ToBase64String(RandomNumberGenerator.GetBytes(16))));
    }

    /// <summary>
    /// Hace el mismo trabajo que una verificación real, para que el tiempo de respuesta
    /// no revele si una boleta tiene cuenta.
    /// </summary>
    public void SimulateVerify(string password) => Verify(password, dummyHash.Value, out _);

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Derive(password, salt, iterations);
        return $"{Algorithm}${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <param name="needsRehash">True si el hash usa otro número de iteraciones y conviene recalcularlo.</param>
    public bool Verify(string password, string storedHash, out bool needsRehash)
    {
        needsRehash = false;

        var parts = storedHash.Split('$');
        if (parts.Length != 4
            || parts[0] != Algorithm
            || !int.TryParse(parts[1], out var storedIterations)
            || storedIterations < 1)
        {
            return false;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Derive(password, salt, storedIterations, expected.Length);

        // Comparación en tiempo constante, para no revelar información por el tiempo de respuesta.
        var matches = CryptographicOperations.FixedTimeEquals(actual, expected);
        needsRehash = matches && storedIterations != iterations;
        return matches;
    }

    private static byte[] Derive(string password, byte[] salt, int iterations, int length = HashSize) =>
        Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, length);
}
