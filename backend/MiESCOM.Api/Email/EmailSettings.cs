using MailKit.Security;

namespace MiESCOM.Api.Email;

/// <summary>
/// Servidor de correo (sección Email de la configuración).
/// En desarrollo, si no se configura, se usa Mailpit de docker-compose (localhost:1025),
/// que atrapa los correos y los muestra en http://localhost:8025 sin enviarlos a nadie.
/// </summary>
public sealed record EmailSettings(
    string Host,
    int Port,
    SecureSocketOptions Security,
    string? User,
    string? Password,
    string FromAddress,
    string FromName,
    string LoginUrl)
{
    public static EmailSettings FromConfiguration(IConfiguration configuration, IHostEnvironment environment)
    {
        var section = configuration.GetSection("Email");
        var host = section["Host"];

        if (string.IsNullOrWhiteSpace(host))
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Falta configurar el servidor de correo (Email:Host) para enviar las contraseñas temporales.");
            }

            return new EmailSettings(
                Host: "localhost",
                Port: 1025,
                Security: SecureSocketOptions.None,
                User: null,
                Password: null,
                FromAddress: section["FromAddress"] ?? "no-responder@miescom.local",
                FromName: section["FromName"] ?? "MiESCOM",
                LoginUrl: section["LoginUrl"] ?? "http://localhost:5173");
        }

        return new EmailSettings(
            Host: host,
            Port: section.GetValue("Port", 587),
            Security: section.GetValue("Security", SecureSocketOptions.StartTls),
            User: section["User"],
            Password: section["Password"],
            FromAddress: section["FromAddress"] ?? throw new InvalidOperationException("Falta Email:FromAddress."),
            FromName: section["FromName"] ?? "MiESCOM",
            LoginUrl: section["LoginUrl"] ?? throw new InvalidOperationException("Falta Email:LoginUrl."));
    }
}
