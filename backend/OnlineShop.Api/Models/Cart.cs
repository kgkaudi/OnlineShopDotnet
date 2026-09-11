namespace OnlineShop.Api.Models;

public class Cart
{
    public string Id { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public List<CartItem> Items { get; set; } = new();
}

public class CartItem
{
    public string Id { get; set; } = default!;
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
}
