namespace Basket.API.Entities
{
    public class Cart
    {
        public string Username { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public Cart()
        {
        }

        public Cart(string username)
        {
            this.Username = username;
        }

        public decimal TotalPrice
        {
            get
            {
                return Items.Sum(x => x.Quantity * x.ItemPrice);
            }
        }
    }
}