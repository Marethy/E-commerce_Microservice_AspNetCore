namespace Shared.DTOs.Product
{
    public class ProductStatisticsDto
    {
        public int TotalProducts { get; set; }
        public int InStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CategoryStatisticsDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public bool HasChildren { get; set; }
    }
}
