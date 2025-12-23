using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Hangfire.API.DTOs;

public class JobListResponse
{
    public List<JobInfo> Jobs { get; set; } = new();
    public JobStatistics Statistics { get; set; } = new();
}

public class JobInfo
{
    public string JobId { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? Method { get; set; }
    public string? Arguments { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Exception { get; set; }
    public string? Reason { get; set; }
}

public class JobStatistics
{
    public long Enqueued { get; set; }
    public long Scheduled { get; set; }
    public long Processing { get; set; }
    public long Succeeded { get; set; }
    public long Failed { get; set; }
    public long Deleted { get; set; }
}
