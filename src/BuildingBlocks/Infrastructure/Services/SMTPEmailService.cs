using Contracts.Services;
using Infrastructure.Configurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using Shared.Services.Email;

public class SMTPEmailService : ISMTPEmailService
{
    private readonly SMTPEmailSetting _settings;
    private readonly ILogger<SMTPEmailService> _logger;

    public SMTPEmailService(SMTPEmailSetting settings, ILogger<SMTPEmailService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task SendEmailAsync(MailRequest request, CancellationToken cancellationToken)
    {
        var emailMessage = new MimeMessage
        {
            Sender = new MailboxAddress(_settings.DisplayName, request.From ?? _settings.From),
            Subject = request.Subject,
            Body = new BodyBuilder { HtmlBody = request.Body }.ToMessageBody()
        };

        if (request.ToAddresses != null && request.ToAddresses.Any())
        {
            foreach (var toAddress in request.ToAddresses)
            {
                emailMessage.To.Add(MailboxAddress.Parse(toAddress));
            }
        }
        else
        {
            emailMessage.To.Add(MailboxAddress.Parse(request.ToAddress));
        }

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_settings.SMTPServer, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            await client.SendAsync(emailMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}