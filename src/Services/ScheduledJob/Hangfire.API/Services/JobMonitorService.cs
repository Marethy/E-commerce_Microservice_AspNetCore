using Hangfire;
using Hangfire.API.DTOs;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using System.Linq;

namespace Hangfire.API.Services;

public interface IJobMonitorService
{
    JobListResponse GetJobs(string? state = null, int page = 0, int pageSize = 20);
    JobInfo? GetJobById(string jobId);
    JobStatistics GetStatistics();
    bool DeleteJob(string jobId);
    bool RequeueJob(string jobId);
}

public class JobMonitorService : IJobMonitorService
{
    private readonly IMonitoringApi _monitoringApi;

    public JobMonitorService()
    {
        _monitoringApi = JobStorage.Current.GetMonitoringApi();
    }

    public JobListResponse GetJobs(string? state = null, int page = 0, int pageSize = 20)
    {
        var jobs = new List<JobInfo>();
        var from = page * pageSize;
        var count = pageSize;

        if (string.IsNullOrEmpty(state) || state.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            jobs.AddRange(GetJobsByState("Scheduled", from, count));
            jobs.AddRange(GetJobsByState("Enqueued", from, count));
            jobs.AddRange(GetJobsByState("Processing", from, count));
            jobs.AddRange(GetJobsByState("Succeeded", from, count));
            jobs.AddRange(GetJobsByState("Failed", from, count));
        }
        else
        {
            jobs.AddRange(GetJobsByState(state, from, count));
        }

        return new JobListResponse
        {
            Jobs = jobs.OrderByDescending(j => j.CreatedAt).Take(pageSize).ToList(),
            Statistics = GetStatistics()
        };
    }

    private List<JobInfo> GetJobsByState(string state, int from, int count)
    {
        var jobs = new List<JobInfo>();

        try
        {
            System.Collections.IEnumerable? jobList = null;

            switch (state.ToLower())
            {
                case "scheduled":
                    jobList = _monitoringApi.ScheduledJobs(from, count);
                    break;
                case "enqueued":
                    jobList = _monitoringApi.EnqueuedJobs("default", from, count);
                    break;
                case "processing":
                    jobList = _monitoringApi.ProcessingJobs(from, count);
                    break;
                case "succeeded":
                    jobList = _monitoringApi.SucceededJobs(from, count);
                    break;
                case "failed":
                    jobList = _monitoringApi.FailedJobs(from, count);
                    break;
                case "deleted":
                    jobList = _monitoringApi.DeletedJobs(from, count);
                    break;
            }

            if (jobList != null)
            {
                foreach (dynamic item in jobList)
                {
                    jobs.Add(MapToJobInfo(item.Key, item.Value));
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error getting jobs for state {state}: {ex.Message}");
        }

        return jobs;
    }

    public JobInfo? GetJobById(string jobId)
    {
        try
        {
            var jobDetails = _monitoringApi.JobDetails(jobId);
            if (jobDetails == null) return null;

            var stateHistory = jobDetails.History.OrderByDescending(h => h.CreatedAt).FirstOrDefault();
            
            return new JobInfo
            {
                JobId = jobId,
                State = stateHistory?.StateName ?? "Unknown",
                Method = jobDetails.Job?.Method?.Name,
                Arguments = jobDetails.Job?.Args != null 
                    ? string.Join(", ", jobDetails.Job.Args.Take(3).Select(a => a?.ToString() ?? "null"))
                    : null,
                CreatedAt = jobDetails.CreatedAt,
                Reason = stateHistory?.Reason
            };
        }
        catch
        {
            return null;
        }
    }

    public JobStatistics GetStatistics()
    {
        var stats = _monitoringApi.GetStatistics();
        
        return new JobStatistics
        {
            Enqueued = stats.Enqueued,
            Scheduled = stats.Scheduled,
            Processing = stats.Processing,
            Succeeded = stats.Succeeded,
            Failed = stats.Failed,
            Deleted = stats.Deleted
        };
    }

    public bool DeleteJob(string jobId)
    {
        try
        {
            return BackgroundJob.Delete(jobId);
        }
        catch
        {
            return false;
        }
    }

    public bool RequeueJob(string jobId)
    {
        try
        {
            return BackgroundJob.Requeue(jobId);
        }
        catch
        {
            return false;
        }
    }

    private JobInfo MapToJobInfo(string jobId, dynamic jobData)
    {
        var info = new JobInfo { JobId = jobId };

        try
        {
            info.State = (string?)jobData.State ?? "Unknown";
            
            if (jobData.CreatedAt != null)
            {
                info.CreatedAt = jobData.CreatedAt;
            }
            
            if (jobData.EnqueueAt != null)
            {
                info.ScheduledAt = jobData.EnqueueAt;
            }
            
            if (jobData.Job != null)
            {
                info.Method = jobData.Job.Method?.Name;
                
                if (jobData.Job.Args != null)
                {
                    var args = jobData.Job.Args as IEnumerable<object>;
                    if (args != null)
                    {
                        info.Arguments = string.Join(", ", args.Take(3).Select(a => a?.ToString() ?? "null"));
                    }
                }
            }
        }
        catch { }

        return info;
    }
}
