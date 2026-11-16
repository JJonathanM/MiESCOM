namespace MiESCOM.Api.Auth;

/// <summary>
/// Tiempo sin actividad tras el cual se cierra la sesión.
/// Se configura en appsettings.json (Session:IdleTimeoutMinutes) y se envía al frontend
/// para que su aviso de inactividad use el mismo valor.
/// </summary>
public sealed record SessionSettings(TimeSpan IdleTimeout)
{
    private const int DefaultIdleTimeoutMinutes = 15;

    public static SessionSettings FromConfiguration(IConfiguration configuration)
    {
        var minutes = configuration.GetValue("Session:IdleTimeoutMinutes", DefaultIdleTimeoutMinutes);
        if (minutes <= 0)
        {
            throw new InvalidOperationException("Session:IdleTimeoutMinutes debe ser mayor que cero.");
        }

        return new SessionSettings(TimeSpan.FromMinutes(minutes));
    }
}
