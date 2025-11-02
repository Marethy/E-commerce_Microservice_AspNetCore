namespace Shared.DTOs.Product
{
    public class SellerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsOfficial { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public decimal Rating { get; set; }
        public int TotalSales { get; set; }
    }

    public class CreateSellerDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsOfficial { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
