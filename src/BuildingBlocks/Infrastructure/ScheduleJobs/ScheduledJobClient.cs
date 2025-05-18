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

public class ScheduledJobClient : IScheduledJobsClient
{
    private readonly IHttpClientHelper _httpClientHelper;
    private readonly UrlSettings _urlSettings;
    private readonly ILogger _logger;

    private static readonly string _scheduledJobs = "api/scheduledJobs";

    public ScheduledJobClient(IHttpClientHelper httpClientHelper, UrlSettings urlSettings, ILogger logger)
    {
        _httpClientHelper = httpClientHelper;
        _urlSettings = urlSettings;
        _logger = logger;
    }

    public async Task<string?> SendReminderEmailAsync(ReminderEmailDto model)
    {
        try
        {
            var httpContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var endpoint = $"{_urlSettings.HangfireUrl}/{_scheduledJobs}/send-reminder-email";

            var result = await _httpClientHelper.SendAsync(
                endpoint,
                httpContent,
                HttpMethod.Post);

            if (!result.EnsureSuccessStatusCode().IsSuccessStatusCode)
            {
                _logger.Error($"SendReminderEmailAsync failed");
                return null;
            }

            var jobId = await result.ReadContentAs();
            _logger.Information($"SendReminderEmailAsync JobId: {jobId}");
            return jobId;
        }
        catch (Exception ex)
        {
            _logger.Error($"SendReminderEmailAsync: {ex.Message}");
            return null;
        }
    }

    public async Task DeleteJobAsync(string jobId)
    {
        try
        {
            var endpoint = $"{_urlSettings.HangfireUrl}/{_scheduledJobs}/delete-job/{jobId}";

            var result = await _httpClientHelper.SendAsync(
                endpoint,
                null,
                HttpMethod.Delete);

            if (!result.EnsureSuccessStatusCode().IsSuccessStatusCode)
            {
                _logger.Error($"DeleteJobAsync failed: {jobId}");
            }

            _logger.Information($"Deleted JobId: {jobId}");
        }
        catch (Exception ex)
        {
            _logger.Error($"DeleteJobAsync: {ex.Message}");
        }
    }
}