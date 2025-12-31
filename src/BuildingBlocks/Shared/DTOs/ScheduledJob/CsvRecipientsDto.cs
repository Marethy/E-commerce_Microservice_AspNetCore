namespace Shared.DTOs.ScheduledJob;

public record ParseCsvRecipientsRequest(
    string CsvContent
);

public record ParseCsvRecipientsResponse(
    List<EmailRecipient> Recipients,
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    List<string> Errors
);
