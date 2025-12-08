namespace Ordering.Application.Common.Models
{
    public class OrderStatistics
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Shipped { get; set; }
        public int Delivered { get; set; }
        public int Cancelled { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
