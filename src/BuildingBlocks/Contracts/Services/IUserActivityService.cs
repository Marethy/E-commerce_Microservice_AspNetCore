using Contracts.Common.Events;

namespace Contracts.Services;

/// <summary>
/// Service for tracking user activities across microservices
/// Used for AI analytics, recommendations, and anomaly detection
/// </summary>
public interface IUserActivityService
{
    /// <summary>
    /// Track a user activity asynchronously
    /// </summary>
    Task TrackActivityAsync(UserActivityEvent activity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Track a user activity with minimal parameters
    /// </summary>
    Task TrackActivityAsync(
        string userId, 
        string entityType, 
        string entityId, 
        string action,
        string correlationId = null,
        Dictionary<string, object> metadata = null,
        CancellationToken cancellationToken = default);
}
