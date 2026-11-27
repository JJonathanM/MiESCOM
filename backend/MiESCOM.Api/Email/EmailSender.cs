using MailKit.Net.Smtp;
using MimeKit;

namespace MiESCOM.Api.Email;

public sealed record EmailMessage(string ToAddress, string ToName, string Subject, string TextBody, string HtmlBody);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

/// <summary>Envía correos por SMTP con MailKit.</summary>
public sealed class SmtpEmailSender(EmailSettings settings) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        mime.To.Add(new MailboxAddress(message.ToName, message.ToAddress));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { TextBody = message.TextBody, HtmlBody = message.HtmlBody }.ToMessageBody();

        using var client = new SmtpClient { Timeout = 15_000 };
        await client.ConnectAsync(settings.Host, settings.Port, settings.Security, cancellationToken);
        if (!string.IsNullOrEmpty(settings.User))
        {
            await client.AuthenticateAsync(settings.User, settings.Password ?? "", cancellationToken);
        }
        await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }
}
