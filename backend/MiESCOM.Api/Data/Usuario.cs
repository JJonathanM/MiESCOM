namespace MiESCOM.Api.Data;

/// <summary>
/// Cuenta de acceso de un alumno. Se crea al registrarse con una boleta que existe en
/// <see cref="Alumno"/>; el correo se toma del alumno, no se guarda aquí.
/// </summary>
public sealed class Usuario
{
    public string Boleta { get; set; } = "";

    /// <summary>Formato pbkdf2-sha256$iteraciones$sal$hash (ver Auth/PasswordHasher.cs).</summary>
    public string PasswordHash { get; set; } = "";

    /// <summary>True mientras el alumno no cambie la contraseña temporal que recibió por correo.</summary>
    public bool PasswordTemporal { get; set; }

    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }

    /// <summary>Cambia con cada cambio de contraseña e invalida las sesiones abiertas.</summary>
    public string SelloSeguridad { get; set; } = "";

    public bool Activo { get; set; } = true;
    public DateTime? UltimoAcceso { get; set; }

    public Alumno Alumno { get; set; } = null!;
}
