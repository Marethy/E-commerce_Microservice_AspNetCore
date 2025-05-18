namespace Basket.API.Entities
{
    public class Cart
    {
        public Cart() { }

        public Cart(string userName)
        {
            UserName = userName;
        }

        public string UserName { get; set; }

        public string EmailAddress { get; set; }

        public List<CartItem> Items { get; set; } = new();

        public decimal TotalPrice => Items.Sum(item => item.ItemPrice * item.Quantity);

        public string JobId { get; set; }
    }
}