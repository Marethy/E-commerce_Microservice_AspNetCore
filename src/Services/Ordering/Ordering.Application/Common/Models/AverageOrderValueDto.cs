namespace Ordering.Application.Common.Models;

public class AverageOrderValueDto
{
    public string Date { get; set; } = string.Empty;
    public decimal AverageValue { get; set; }
}
