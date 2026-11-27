using System.Net;

namespace MiESCOM.Api.Email;

/// <summary>Correo con la contraseña temporal que recibe el alumno al registrarse.</summary>
public static class CorreoContrasenaTemporal
{
    public static EmailMessage Crear(string correo, string nombre, string boleta, string contrasena, string loginUrl)
    {
        const string asunto = "Tu contraseña temporal de MiESCOM";

        var texto = $"""
            Hola, {nombre}:

            Se creó tu cuenta de MiESCOM con la boleta {boleta}.

            Tu contraseña temporal es: {contrasena}

            Entra en {loginUrl} con tu boleta y esta contraseña. Al entrar te pediremos cambiarla.

            Si no solicitaste esta cuenta, ignora este correo.

            Escuela Superior de Cómputo · Instituto Politécnico Nacional
            """;

        var n = WebUtility.HtmlEncode(nombre);
        var b = WebUtility.HtmlEncode(boleta);
        var c = WebUtility.HtmlEncode(contrasena);
        var u = WebUtility.HtmlEncode(loginUrl);

        var html = $"""
            <!doctype html>
            <html lang="es">
            <body style="margin:0;padding:24px;background:#f5f7f9;font-family:Lato,Arial,sans-serif;color:#333333;">
              <div style="max-width:520px;margin:0 auto;background:#ffffff;border-top:4px solid #006699;border-radius:8px;padding:24px;">
                <h1 style="margin:0 0 16px;font-size:20px;color:#006699;">MiESCOM</h1>
                <p>Hola, {n}:</p>
                <p>Se creó tu cuenta de MiESCOM con la boleta <strong>{b}</strong>.</p>
                <p>Tu contraseña temporal es:</p>
                <p style="font-family:Consolas,monospace;font-size:22px;letter-spacing:2px;background:#e3f1f8;color:#00405f;padding:12px 16px;border-radius:6px;display:inline-block;">{c}</p>
                <p>Entra en <a href="{u}" style="color:#006699;">{u}</a> con tu boleta y esta contraseña. Al entrar te pediremos cambiarla.</p>
                <p style="color:#55606e;font-size:13px;">Si no solicitaste esta cuenta, ignora este correo.</p>
                <p style="color:#55606e;font-size:13px;margin-bottom:0;">Escuela Superior de Cómputo · Instituto Politécnico Nacional</p>
              </div>
            </body>
            </html>
            """;

        return new EmailMessage(correo, nombre, asunto, texto, html);
    }

    /// <summary>Oculta parte del correo para mostrarlo en pantalla: a*****o@example.com.</summary>
    public static string Enmascarar(string correo)
    {
        var at = correo.IndexOf('@');
        if (at <= 0)
        {
            return "***";
        }

        var local = correo[..at];
        var visible = local.Length <= 2
            ? $"{local[0]}*"
            : $"{local[0]}{new string('*', Math.Min(local.Length - 2, 6))}{local[^1]}";
        return visible + correo[at..];
    }
}
