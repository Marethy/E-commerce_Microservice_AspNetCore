using Contracts.Domains;

namespace Product.API.Entities
{
    public class CategoryStatisticsCache : EntityBase<Guid>
    {
        public Guid CategoryId { get; set; }
        public int ProductCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public Category Category { get; set; } = null!;
    }
}
