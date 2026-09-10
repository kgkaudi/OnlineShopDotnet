namespace OnlineShop.Api.Models;

public class Cart
{
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public List<CartItem> Items { get; set; } = new();
}

public class CartItem
{
    public string ProductId { get; set; } = null!;
    public int Quantity { get; set; }
}
