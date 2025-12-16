using Customer.API.Entities;
using Customer.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.SeedWork.ApiResult;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace Customer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _repository;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationRepository repository,
            ILogger<NotificationsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? User.FindFirstValue("sub") 
                ?? User.FindFirstValue(ClaimTypes.Email) 
                ?? throw new UnauthorizedAccessException("User ID not found");
        }

        /// <summary>
        /// Get user's notifications with filtering and pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<object>>> GetNotifications(
            [FromQuery] int page = 0,
            [FromQuery] int size = 20,
            [FromQuery] string? type = null,
            [FromQuery] bool? isRead = null)
        {
            var userId = GetUserId();
            var notifications = await _repository.GetUserNotificationsAsync(userId, page, size, type, isRead);
            var unreadCount = await _repository.GetUnreadCountAsync(userId);

            var result = new
            {
                content = notifications,
                page,
                size,
                totalElements = notifications.Count(),
                unreadCount,
                hasMore = notifications.Count() >= size
            };

            return Ok(new ApiSuccessResult<object>(result));
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResult<int>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<int>>> GetUnreadCount()
        {
            var userId = GetUserId();
            var count = await _repository.GetUnreadCountAsync(userId);
            return Ok(new ApiSuccessResult<int>(count));
        }

        /// <summary>
        /// Mark single notification as read
        /// </summary>
        [HttpPut("{id}/read")]
        [ProducesResponseType(typeof(ApiResult<bool>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<bool>>> MarkAsRead([Required] Guid id)
        {
            var userId = GetUserId();
            var notification = await _repository.GetByIdAsync(id);

            if (notification == null)
            {
                return NotFound(new ApiErrorResult<bool>("Notification not found"));
            }

            if (notification.UserId != userId)
            {
                return Forbid();
            }

            await _repository.MarkAsReadAsync(id);
            return Ok(new ApiSuccessResult<bool>(true));
        }

        /// <summary>
        /// Mark multiple notifications as read
        /// </summary>
        [HttpPut("mark-read")]
        [ProducesResponseType(typeof(ApiResult<bool>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<bool>>> MarkMultipleAsRead([FromBody] List<Guid> notificationIds)
        {
            var userId = GetUserId();

            foreach (var id in notificationIds)
            {
                var notification = await _repository.GetByIdAsync(id);
                if (notification != null && notification.UserId == userId)
                {
                    await _repository.MarkAsReadAsync(id);
                }
            }

            return Ok(new ApiSuccessResult<bool>(true));
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPut("mark-all-read")]
        [ProducesResponseType(typeof(ApiResult<bool>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<bool>>> MarkAllAsRead()
        {
            var userId = GetUserId();
            await _repository.MarkAllAsReadAsync(userId);
            return Ok(new ApiSuccessResult<bool>(true));
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResult<bool>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<bool>>> DeleteNotification([Required] Guid id)
        {
            var userId = GetUserId();
            var notification = await _repository.GetByIdAsync(id);

            if (notification == null)
            {
                return NotFound(new ApiErrorResult<bool>("Notification not found"));
            }

            if (notification.UserId != userId)
            {
                return Forbid();
            }

            await _repository.DeleteAsync(id);
            return Ok(new ApiSuccessResult<bool>(true));
        }

        /// <summary>
        /// Admin: Send notification to specific user
        /// </summary>
        [HttpPost("admin/send")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResult<Notification>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<Notification>>> SendNotification([FromBody] CreateNotificationRequest request)
        {
            var notification = new Notification
            {
                UserId = request.UserId,
                Type = request.Type,
                Priority = request.Priority ?? "medium",
                Title = request.Title,
                Message = request.Message,
                ActionUrl = request.ActionUrl,
                ActionLabel = request.ActionLabel,
                ImageUrl = request.ImageUrl,
                Metadata = request.Metadata,
                NotificationDate = DateTimeOffset.UtcNow
            };

            var created = await _repository.CreateAsync(notification);
            _logger.LogInformation("Notification sent to user {UserId}: {Title}", request.UserId, request.Title);

            return Ok(new ApiSuccessResult<Notification>(created));
        }

        /// <summary>
        /// Admin: Broadcast notification to multiple users
        /// </summary>
        [HttpPost("admin/broadcast")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<object>>> BroadcastNotification([FromBody] BroadcastNotificationRequest request)
        {
            var notifications = request.UserIds.Select(userId => new Notification
            {
                UserId = userId,
                Type = request.Type,
                Priority = request.Priority ?? "medium",
                Title = request.Title,
                Message = request.Message,
                ActionUrl = request.ActionUrl,
                ActionLabel = request.ActionLabel,
                ImageUrl = request.ImageUrl,
                Metadata = request.Metadata,
                NotificationDate = DateTimeOffset.UtcNow
            }).ToList();

            await _repository.CreateBulkAsync(notifications);
            _logger.LogInformation("Broadcasted notification to {Count} users: {Title}", request.UserIds.Count, request.Title);

            return Ok(new ApiSuccessResult<object>(new { count = notifications.Count, message = "Notifications sent successfully" }));
        }
    }

    // DTOs
    public class CreateNotificationRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string Type { get; set; } = string.Empty;
        public string? Priority { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public string? ActionLabel { get; set; }
        public string? ImageUrl { get; set; }
        public string? Metadata { get; set; }
    }

    public class BroadcastNotificationRequest
    {
        [Required]
        public List<string> UserIds { get; set; } = new();
        [Required]
        public string Type { get; set; } = string.Empty;
        public string? Priority { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public string? ActionLabel { get; set; }
        public string? ImageUrl { get; set; }
        public string? Metadata { get; set; }
    }
}
