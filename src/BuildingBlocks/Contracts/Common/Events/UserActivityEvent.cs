using EventBus.Messages;

namespace Contracts.Common.Events;

/// <summary>
/// Event to track user activities across microservices for AI analytics
/// Essential for Recommendation Engine, Chatbot, and Anomaly Detection
/// </summary>
public record UserActivityEvent : IntegrationBaseEvent
{
    /// <summary>
    /// User identifier (username or userId)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of entity being interacted with (Product, Order, Basket, Customer)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Identifier of the entity
    /// </summary>
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of action performed (View, AddToCart, Purchase, Search, Remove, Update)
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Correlation ID for tracking across services
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional metadata for AI context
    /// Examples: SessionId, Source (Web/Mobile), SearchQuery, Price, Quantity, etc.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Timestamp when activity occurred (inherited from IntegrationBaseEvent.CreationDate)
    /// </summary>
    public DateTime ActivityTimestamp => CreationDate;
}

/// <summary>
/// Common action types for user activities
/// </summary>
public static class UserActivityActions
{
    public const string View = "View";
    public const string AddToCart = "AddToCart";
    public const string RemoveFromCart = "RemoveFromCart";
    public const string Purchase = "Purchase";
    public const string Search = "Search";
    public const string Filter = "Filter";
    public const string Review = "Review";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Checkout = "Checkout";
    public const string Login = "Login";
    public const string Logout = "Logout";
}

/// <summary>
/// Common entity types for user activities
/// </summary>
public static class UserActivityEntityTypes
{
    public const string Product = "Product";
    public const string Category = "Category";
    public const string Basket = "Basket";
    public const string Order = "Order";
    public const string Customer = "Customer";
    public const string Review = "Review";
}
