// src/Services/ScheduledJob/Hangfire.API/Controllers/EmailCampaignController.cs
using Hangfire.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.ScheduledJob;

namespace Hangfire.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmailCampaignController : ControllerBase
{
    private readonly IBackgroundJobService _backgroundJobService;

    public EmailCampaignController(IBackgroundJobService backgroundJobService)
    {
        _backgroundJobService = backgroundJobService;
    }

    /// <summary>
    /// Schedule promotional email campaign
    /// </summary>
    [HttpPost("schedule-promotion")]
    public IActionResult SchedulePromotionalEmail([FromBody] PromotionalEmailDto model)
    {
        var jobId = _backgroundJobService.SendMailContent(
model.Email,
      model.Subject,
  model.Content,
          model.ScheduledAt
  );
     
        return Ok(new { jobId, message = "Promotional email scheduled successfully" });
    }

    /// <summary>
    /// Schedule bulk promotional emails (for campaigns)
    /// </summary>
    [HttpPost("schedule-bulk-promotion")]
    public IActionResult ScheduleBulkPromotionalEmails([FromBody] BulkPromotionalEmailDto model)
    {
        var jobIds = new List<string>();
      
        foreach (var recipient in model.Recipients)
   {
        var personalizedContent = model.ContentTemplate
       .Replace("{{CustomerName}}", recipient.Name)
   .Replace("{{DiscountCode}}", recipient.DiscountCode);
            
 var jobId = _backgroundJobService.SendMailContent(
    recipient.Email,
        model.Subject,
     personalizedContent,
     model.ScheduledAt
          );
       
            jobIds.Add(jobId);
        }
        
return Ok(new 
  { 
    totalScheduled = jobIds.Count, 
       jobIds,
        message = $"Bulk promotional emails scheduled for {model.ScheduledAt}"
        });
    }

    /// <summary>
    /// Cancel scheduled promotional email
    /// </summary>
  [HttpDelete("cancel/{jobId}")]
  public IActionResult CancelPromotionalEmail(string jobId)
    {
        var result = _backgroundJobService.ScheduledJobService.Delete(jobId);
        return Ok(new { success = result, message = result ? "Email cancelled" : "Job not found" });
    }
}
