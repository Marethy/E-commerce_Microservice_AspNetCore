using Contracts.Common.Interfaces;
using Contracts.ScheduledJobs;
using Infrastructure.Extensions;
using MongoDB.Driver;
using Serilog;
using Shared.Configurations;
using Shared.DTOs.ScheduledJob;
using System.Text;
using System.Text.Json;

namespace Infrastructure.ScheduleJobs;

public class ScheduledJobClient(IHttpClientHelper httpClientHelper, UrlSettings urlSettings, ILogger logger) : IScheduledJobsClient
{
    private static readonly string _scheduledJobs = "api/scheduledJobs";

    public async Task<string?> SendReminderEmailAsync(ReminderEmailDto model)
    {
        try
        {
            var httpContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var endpoint = $"{urlSettings.HangfireUrl}/{_scheduledJobs}/send-reminder-email";

            var result = await httpClientHelper.SendAsync(
                endpoint,
                httpContent,
                HttpMethod.Post);

            if (!result.EnsureSuccessStatusCode().IsSuccessStatusCode)
            {
                logger.Error($"SendReminderEmailAsync failed");
                return null;
            }

            var jobId = await result.ReadContentAs();
            logger.Information($"SendReminderEmailAsync JobId: {jobId}");
            return jobId;
        }
        catch (Exception ex)
        {
            logger.Error($"SendReminderEmailAsync: {ex.Message}");
            return null;
        }
    }

    public async Task DeleteJobAsync(string jobId)
    {
        try
        {
            var endpoint = $"{urlSettings.HangfireUrl}/{_scheduledJobs}/delete-job/{jobId}";

            var result = await httpClientHelper.SendAsync(
                endpoint,
                null,
                HttpMethod.Delete);

            if (!result.EnsureSuccessStatusCode().IsSuccessStatusCode)
            {
                logger.Error($"DeleteJobAsync failed: {jobId}");
            }

            logger.Information($"Deleted JobId: {jobId}");
        }
        catch (Exception ex)
        {
            logger.Error($"DeleteJobAsync: {ex.Message}");
        }
    }
}