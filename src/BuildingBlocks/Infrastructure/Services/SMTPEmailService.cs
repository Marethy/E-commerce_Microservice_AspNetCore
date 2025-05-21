using Contracts.Services;
using Infrastructure.Configurations;
using MailKit.Net.Smtp;
using MimeKit;
using Serilog;
using Shared.Services.Email;

namespace Infrastructure.Services;

public class SMTPEmailService(ILogger logger, SMTPEmailSetting emailSetting) : ISMTPEmailService
{
    private readonly SmtpClient _smtpClient = new SmtpClient();

    public async Task SendEmailAsync(MailRequest request, CancellationToken cancellationToken = default)
    {
        var emailMessage = GetEmailMessage(request);

        try
        {
            await _smtpClient.ConnectAsync(emailSetting.SMTPServer, emailSetting.Port,
                emailSetting.UseSsl, cancellationToken);
            await _smtpClient.AuthenticateAsync(emailSetting.Username, emailSetting.Password, cancellationToken);
            await _smtpClient.SendAsync(emailMessage, cancellationToken);
            await _smtpClient.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message, ex);
        }
        finally
        {
            await _smtpClient.DisconnectAsync(true, cancellationToken);
            _smtpClient.Dispose();
        }
    }

    public void SendEmail(MailRequest request)
    {
        var emailMessage = GetEmailMessage(request);

        try
        {
            _smtpClient.Connect(emailSetting.SMTPServer, emailSetting.Port,
                emailSetting.UseSsl);
            _smtpClient.Authenticate(emailSetting.Username, emailSetting.Password);
            _smtpClient.Send(emailMessage);
            _smtpClient.Disconnect(true);
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message, ex);
        }
        finally
        {
            _smtpClient.Disconnect(true);
            _smtpClient.Dispose();
        }
    }

    private MimeMessage GetEmailMessage(MailRequest request)
    {
        var emailMessage = new MimeMessage
        {
            Sender = new MailboxAddress(emailSetting.DisplayName, request.From ?? emailSetting.From),
            Subject = request.Subject,
            Body = new BodyBuilder
            {
                HtmlBody = request.Body
            }.ToMessageBody()
        };

        if (request.ToAddresses.Any())
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

        return emailMessage;
    }
}