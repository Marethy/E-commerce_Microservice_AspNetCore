using Contracts.Domains;

namespace Product.API.Entities
{
    public class ProductStatisticsCache : EntityBase<string>
    {
        public string Value { get; set; } = string.Empty;
        public DateTimeOffset LastUpdated { get; set; }
    }
}
