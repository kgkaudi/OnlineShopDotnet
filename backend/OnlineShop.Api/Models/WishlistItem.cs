namespace OnlineShop.Api.Models;

public class WishlistItem
{
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
