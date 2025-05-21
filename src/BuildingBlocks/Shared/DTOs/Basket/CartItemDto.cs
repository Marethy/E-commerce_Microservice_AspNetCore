using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Basket;

public class CartItemDto
{
    [Required]
    [Range(1, double.PositiveInfinity, ErrorMessage = "The field {0} must be >= {1}.")]
    public int Quantity { get; set; }

    [Required]
    [Range(0.1, double.PositiveInfinity, ErrorMessage = "The field {0} must be >= {1}.")]
    public decimal ItemPrice { get; set; }

    [Required]
    public string ItemNo { get; set; }

    [Required]
    public string ItemName { get; set; }

    public int AvailableQuantity { get; set; }

    public void SetAvailableQuantity(int quantity)
    {
        AvailableQuantity = quantity;
    }
}