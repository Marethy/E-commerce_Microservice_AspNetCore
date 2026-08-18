namespace Ordering.Application.Features.V1.Orders;

public class CreateOrUpdateCommand
{
    public decimal TotalPrice { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string EmailAddress { get; set; }

    public string ShippingAddress { get; set; }

    private string _invoiceAddress;

    public string InvoiceAddress
    {
        get => _invoiceAddress;
        set => _invoiceAddress = value ?? ShippingAddress;
    }
}