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
                    // Use JobDetails for complete information
                    var jobInfo = GetJobById(item.Key);
                    if (jobInfo != null)
                    {
                        jobs.Add(jobInfo);
                    }
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
            
            var info = new JobInfo
            {
                JobId = jobId,
                State = stateHistory?.StateName ?? "Unknown",
                Method = jobDetails.Job?.Method?.Name,
                CreatedAt = jobDetails.CreatedAt,
                Reason = stateHistory?.Reason
            };

            // Get state-specific timestamps
            foreach (var history in jobDetails.History.OrderByDescending(h => h.CreatedAt))
            {
                switch (history.StateName?.ToLower())
                {
                    case "scheduled":
                        if (info.ScheduledAt == null && history.Data.TryGetValue("EnqueueAt", out var enqueueAt))
                        {
                            try
                            {
                                // Unix timestamp in milliseconds
                                if (long.TryParse(enqueueAt, out var unixMs))
                                {
                                    info.ScheduledAt = DateTimeOffset.FromUnixTimeMilliseconds(unixMs).DateTime;
                                }
                                else
                                {
                                    info.ScheduledAt = DateTime.Parse(enqueueAt);
                                }
                            }
                            catch { }
                        }
                        break;
                    case "processing":
                        if (info.StartedAt == null)
                        {
                            info.StartedAt = history.CreatedAt;
                        }
                        break;
                    case "succeeded":
                    case "failed":
                    case "deleted":
                        if (info.CompletedAt == null)
                        {
                            info.CompletedAt = history.CreatedAt;
                        }
                        if (history.StateName?.ToLower() == "failed" && history.Data.TryGetValue("ExceptionMessage", out var exceptionMsg))
                        {
                            info.Exception = exceptionMsg;
                        }
                        break;
                }
            }

            // Serialize arguments properly
            if (jobDetails.Job?.Args != null && jobDetails.Job.Args.Count > 0)
            {
                var argsList = new List<string>();
                foreach (var arg in jobDetails.Job.Args.Take(3))
                {
                    if (arg == null)
                    {
                        argsList.Add("null");
                    }
                    else if (arg is string || arg.GetType().IsPrimitive)
                    {
                        argsList.Add(arg.ToString() ?? "null");
                    }
                    else
                    {
                        try
                        {
                            var json = System.Text.Json.JsonSerializer.Serialize(arg, new System.Text.Json.JsonSerializerOptions 
                            { 
                                WriteIndented = false,
                                MaxDepth = 3,
                                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                            });
                            argsList.Add(json);
                        }
                        catch
                        {
                            argsList.Add($"<{arg.GetType().Name}>");
                        }
                    }
                }
                info.Arguments = string.Join(", ", argsList);
            }

            return info;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error getting job {jobId}: {ex.Message}");
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
            // Log the actual type for debugging
            var dataType = jobData.GetType();
            System.Console.WriteLine($"[DEBUG] Job {jobId} - Type: {dataType.Name}, Full: {dataType.FullName}");

            // Try to get the state name from various possible properties
            string stateName = "Unknown";
            
            // Check for common state property names
            try
            {
                if (jobData.GetType().GetProperty("State") != null)
                {
                    stateName = jobData.State?.ToString() ?? "Unknown";
                }
            }
            catch { }

            // Determine state from object type name if State property doesn't exist
            string typeName = dataType.Name.ToLower();
            if (typeName.Contains("scheduled"))
            {
                stateName = "Scheduled";
                try
                {
                    info.ScheduledAt = jobData.EnqueueAt;
                    if (jobData.InScheduledState != null)
                    {
                        info.CreatedAt = jobData.InScheduledState.ScheduledAt;
                    }
                }
                catch (Exception ex) 
                {
                    System.Console.WriteLine($"[DEBUG] Error accessing Scheduled props: {ex.Message}");
                }
            }
            else if (typeName.Contains("enqueued"))
            {
                stateName = "Enqueued";
                try
                {
                    if (jobData.InEnqueuedState != null)
                    {
                        info.CreatedAt = jobData.InEnqueuedState.EnqueuedAt;
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[DEBUG] Error accessing Enqueued props: {ex.Message}");
                }
            }
            else if (typeName.Contains("processing"))
            {
                stateName = "Processing";
                try
                {
                    info.StartedAt = jobData.StartedAt;
                    if (jobData.InProcessingState != null)
                    {
                        info.CreatedAt = jobData.InProcessingState.StartedAt;
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[DEBUG] Error accessing Processing props: {ex.Message}");
                }
            }
            else if (typeName.Contains("succeeded"))
            {
                stateName = "Succeeded";
                try
                {
                    info.CompletedAt = jobData.SucceededAt;
                    
                    // Try to get timestamps from the InSucceededState
                    if (jobData.InSucceededState != null)
                    {
                        try { info.CreatedAt = jobData.InSucceededState.SucceededAt; } catch { }
                    }
                    
                    // Try to get from job metadata
                    if (jobData.Job != null && jobData.Job.CreatedAt != null)
                    {
                        info.CreatedAt = jobData.Job.CreatedAt;
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[DEBUG] Error accessing Succeeded props: {ex.Message}");
                }
            }
            else if (typeName.Contains("failed"))
            {
                stateName = "Failed";
                try
                {
                    info.CompletedAt = jobData.FailedAt;
                    if (jobData.InFailedState != null)
                    {
                        info.CreatedAt = jobData.InFailedState.FailedAt;
                        info.Exception = jobData.InFailedState.ExceptionMessage;
                        info.Reason = jobData.InFailedState.Reason;
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[DEBUG] Error accessing Failed props: {ex.Message}");
                }
            }
            else if (typeName.Contains("deleted"))
            {
                stateName = "Deleted";
                try
                {
                    info.CompletedAt = jobData.DeletedAt;
                    if (jobData.InDeletedState != null)
                    {
                        info.CreatedAt = jobData.InDeletedState.DeletedAt;
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[DEBUG] Error accessing Deleted props: {ex.Message}");
                }
            }

            info.State = stateName;

            // Try to extract job method and arguments
            try
            {
                if (jobData.Job != null)
                {
                    info.Method = jobData.Job.Method?.Name;
                    
                    if (jobData.Job.Args != null)
                    {
                        var argsList = new List<string>();
                        foreach (var arg in jobData.Job.Args)
                        {
                            if (argsList.Count >= 3) break;
                            
                            // Try to serialize complex objects to JSON
                            string argValue;
                            if (arg == null)
                            {
                                argValue = "null";
                            }
                            else if (arg is string || arg.GetType().IsPrimitive)
                            {
                                argValue = arg.ToString() ?? "null";
                            }
                            else
                            {
                                try
                                {
                                    argValue = System.Text.Json.JsonSerializer.Serialize(arg, new System.Text.Json.JsonSerializerOptions 
                                    { 
                                        WriteIndented = false,
                                        MaxDepth = 2
                                    });
                                }
                                catch
                                {
                                    argValue = arg.ToString() ?? arg.GetType().Name;
                                }
                            }
                            
                            argsList.Add(argValue);
                        }
                        if (argsList.Count > 0)
                        {
                            info.Arguments = string.Join(", ", argsList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DEBUG] Error accessing Job props: {ex.Message}");
            }

            // Fallback: try direct property access for timestamps
            if (info.CreatedAt == null)
            {
                try { info.CreatedAt = jobData.CreatedAt; } catch { }
            }
            
            System.Console.WriteLine($"[DEBUG] Job {jobId} mapped - State: {info.State}, Method: {info.Method}, CreatedAt: {info.CreatedAt}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error mapping job {jobId}: {ex.Message}");
        }

        return info;
    }
}
