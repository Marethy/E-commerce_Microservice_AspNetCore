using Shared.DTOs.ScheduledJob;

namespace Contracts.ScheduledJobs;

public interface IScheduledJobsClient
{
    Task<string?> SendReminderEmailAsync(ReminderEmailDto model);
    Task DeleteJobAsync(string jobId);
}