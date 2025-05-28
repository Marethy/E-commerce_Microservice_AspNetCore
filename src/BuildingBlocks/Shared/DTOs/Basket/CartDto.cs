
namespace Shared.DTOs.Basket;
public class CartDto
{
    public string UserName { get; set; }

    public string EmailAddress { get; set; }

    public List<CartItemDto> Items { get; set; } = new();

    public decimal TotalPrice => Items.Sum(item => item.ItemPrice * item.Quantity);

    public CartDto() { }

    public CartDto(string userName) { UserName = userName; }
}
