namespace Shared.DTOs.ScheduledJob;

public record ReminderEmailDto(string Email, string Subject, string Content, DateTimeOffset EnqueueAt);