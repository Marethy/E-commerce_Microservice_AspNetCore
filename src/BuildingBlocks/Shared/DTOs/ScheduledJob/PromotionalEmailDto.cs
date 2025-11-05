// src/BuildingBlocks/Shared/DTOs/ScheduledJob/PromotionalEmailDto.cs
namespace Shared.DTOs.ScheduledJob;

public record PromotionalEmailDto(
    string Email,
    string Subject,
    string Content,
    DateTimeOffset ScheduledAt,
    string? CampaignId = null,
    Dictionary<string, string>? Metadata = null
);

public record BulkPromotionalEmailDto(
    string Subject,
    string ContentTemplate, // With placeholders like {{CustomerName}}
    DateTimeOffset ScheduledAt,
    List<EmailRecipient> Recipients,
    string? CampaignId = null
);

public record EmailRecipient(
    string Email,
    string Name,
    string? DiscountCode = null,
    Dictionary<string, string>? CustomFields = null
);
