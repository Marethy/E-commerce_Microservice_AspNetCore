namespace Ordering.Application.Common.Models;

public class RevenueByStatusDto
{
    public string StatusName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Count { get; set; }
}
