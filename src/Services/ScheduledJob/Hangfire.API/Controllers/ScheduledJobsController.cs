using Hangfire.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.ScheduledJob;

namespace Hangfire.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ScheduledJobsController(IBackgroundJobService backgroundJobService) : ControllerBase
{
    [HttpPost]
    [Route("send-reminder-email")]
    public IActionResult SendReminderEmail(ReminderEmailDto model)
    {
        var jobId = backgroundJobService.SendMailContent(model.Email, model.Subject, model.Content, model.EnqueueAt);
        return Ok(jobId);
    }

    [HttpDelete]
    [Route("delete-job/{id}")]
    public IActionResult DeleteJob(string id)
    {
        var result = backgroundJobService.ScheduledJobService.Delete(id);
        return Ok(result);
    }
}