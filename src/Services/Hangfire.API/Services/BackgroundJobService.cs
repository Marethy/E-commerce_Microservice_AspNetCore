using Contracts.ScheduledJobs;
using Contracts.Services;
using Hangfire.API.Services.Interfaces;
using Shared.Services.Email;
using System.Linq.Expressions;
using ILogger = Serilog.ILogger;

namespace Hangfire.API.Services;

public class BackgroundJobService(IScheduledJobService jobService, ISMTPEmailService emailSMTPService, ILogger logger) : IBackgroundJobService
{
    public IScheduledJobService ScheduledJobService => jobService;

    public string? SendMailContent(string email, string subject, string emailContent, DateTimeOffset enqueueAt)
    {
        var emailRequest = new MailRequest
        {
            ToAddress = email,
            Subject = subject,
            Body = emailContent
        };

        try
        {
            Expression<Action> emailJob = () => SendEmail(emailRequest);
            var jobId = jobService.Schedule(emailJob, enqueueAt);
            logger.Information($"Sent email to {email} with subject: {subject} - Job Id: {jobId}");

            return jobId;
        }
        catch (Exception ex)
        {
            logger.Error($"Failed due to an error with the email service: {ex.Message}");
        }

        return null;
    }

    // Wrapper method to avoid optional arguments in the expression tree
    public  void SendEmail(MailRequest emailRequest)
    {
        emailSMTPService.SendEmailAsync(emailRequest).GetAwaiter().GetResult();
    }
}
