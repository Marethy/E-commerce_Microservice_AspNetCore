using System.ComponentModel.DataAnnotations;

namespace Basket.API.Entities
{
    public class BasketCheckout
    {
        [Required]
        public string Username { get; set; }

        public decimal TotalPrice { get; set; }


        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public string ShippingAddress { get; set; }

        private string _invoiceAddress;

        public string? InvoiceAddress
        {
            get => _invoiceAddress;
            set => _invoiceAddress = value ?? ShippingAddress;
        }
    }
}