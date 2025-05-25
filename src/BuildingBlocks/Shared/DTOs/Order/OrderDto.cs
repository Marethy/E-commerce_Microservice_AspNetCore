using Shared.Enums.Order;

namespace Shared.DTOs.Order;

public class OrderDto
{
    public long Id { get; set; }

    public string DocumentNo { get; set; }

    public string UserName { get; set; }

    public decimal TotalPrice { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string EmailAddress { get; set; }

    public string ShippingAddress { get; set; }

    public string InvoiceAddress { get; set; }

    public OrderStatus Status { get; set; }
}
