using Contracts.ScheduledJobs;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Hangfire.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WelcomeController : ControllerBase
{
    private readonly IScheduledJobService _scheduledJobService;
    private readonly ILogger _logger;

    public WelcomeController(IScheduledJobService scheduledJobService, ILogger logger)
    {
        _scheduledJobService = scheduledJobService;
        _logger = logger;
    }

    [HttpPost]
    [Route("[action]")]
    public IActionResult Welcome()
    {
        var jobId = _scheduledJobService.Enqueue(() => ResponseWelcome("Welcome to Hangfire API"));
        return Ok($"Job ID: {jobId} - Enqueue Job");
    }

    [HttpPost]
    [Route("[action]")]
    public IActionResult DelayedWelcome()
    {
        var seconds = 5;
        var jobId = _scheduledJobService.Schedule(() => ResponseWelcome("Welcome to Hangfire API"),
            TimeSpan.FromSeconds(seconds));
        return Ok($"Job ID: {jobId} - Delayed Job");
    }

    [HttpPost]
    [Route("[action]")]
    public IActionResult WelcomeAt()
    {
        var enqueueAt = DateTimeOffset.UtcNow.AddSeconds(10);
        var jobId = _scheduledJobService.Schedule(() => ResponseWelcome("Welcome to Hangfire API"),
            enqueueAt);
        return Ok($"Job ID: {jobId} - Schedule Job");
    }

    [HttpPost]
    [Route("[action]")]
    public IActionResult ConfirmedWelcome()
    {
        const int timeInSeconds = 5;

        var parentJobId = _scheduledJobService.Schedule(() => ResponseWelcome("Welcome to Hangfire API"),
            TimeSpan.FromSeconds(timeInSeconds));

        var jobId = _scheduledJobService.ContinueQueueWith(parentJobId,
            () => ResponseWelcome("Welcome message is sent"));

        return Ok($"Job ID: {jobId} - Confirmed Welcome will be sent in {timeInSeconds} seconds");
    }

    [NonAction]
    public void ResponseWelcome(string text)
    {
        _logger.Information(text);
    }
}