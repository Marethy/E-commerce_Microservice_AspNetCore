using Contracts.Common.Events;
using Contracts.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Implementation of user activity tracking service
/// Publishes events to RabbitMQ for AI consumption
/// </summary>
public class UserActivityService : IUserActivityService
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UserActivityService> _logger;

    public UserActivityService(IPublishEndpoint publishEndpoint, ILogger<UserActivityService> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task TrackActivityAsync(UserActivityEvent activity, CancellationToken cancellationToken = default)
    {
        try
        {
            // Publish to event bus for AI consumption
            await _publishEndpoint.Publish(activity, cancellationToken);

            _logger.LogInformation(
                "User activity tracked: {Action} on {EntityType}/{EntityId} by user {UserId} [CorrelationId: {CorrelationId}]",
                activity.Action, activity.EntityType, activity.EntityId, activity.UserId, activity.CorrelationId);
        }
        catch (Exception ex)
        {
            // Don't fail the request if activity tracking fails
            _logger.LogError(ex, 
                "Failed to track user activity: {Action} on {EntityType}/{EntityId} by user {UserId}",
                activity.Action, activity.EntityType, activity.EntityId, activity.UserId);
        }
    }

    public async Task TrackActivityAsync(
        string userId,
        string entityType,
        string entityId,
        string action,
        string correlationId = null,
        Dictionary<string, object> metadata = null,
        CancellationToken cancellationToken = default)
    {
        var activity = new UserActivityEvent
        {
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            CorrelationId = correlationId ?? string.Empty,
            Metadata = metadata ?? new Dictionary<string, object>()
        };

        await TrackActivityAsync(activity, cancellationToken);
    }
}
