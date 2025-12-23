using Hangfire.API.DTOs;
using Hangfire.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hangfire.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobMonitorController : ControllerBase
{
    private readonly IJobMonitorService _jobMonitorService;

    public JobMonitorController(IJobMonitorService jobMonitorService)
    {
        _jobMonitorService = jobMonitorService;
    }

    [HttpGet("jobs")]
    public IActionResult GetJobs([FromQuery] string? state = null, [FromQuery] int page = 0, [FromQuery] int pageSize = 20)
    {
        var result = _jobMonitorService.GetJobs(state, page, pageSize);
        return Ok(result);
    }

    [HttpGet("jobs/{jobId}")]
    public IActionResult GetJob(string jobId)
    {
        var job = _jobMonitorService.GetJobById(jobId);
        if (job == null)
        {
            return NotFound(new { message = "Job not found" });
        }
        return Ok(job);
    }

    [HttpGet("statistics")]
    public IActionResult GetStatistics()
    {
        var stats = _jobMonitorService.GetStatistics();
        return Ok(stats);
    }

    [HttpDelete("jobs/{jobId}")]
    public IActionResult DeleteJob(string jobId)
    {
        var result = _jobMonitorService.DeleteJob(jobId);
        return Ok(new { success = result, message = result ? "Job deleted" : "Failed to delete job" });
    }

    [HttpPost("jobs/{jobId}/requeue")]
    public IActionResult RequeueJob(string jobId)
    {
        var result = _jobMonitorService.RequeueJob(jobId);
        return Ok(new { success = result, message = result ? "Job requeued" : "Failed to requeue job" });
    }
}
