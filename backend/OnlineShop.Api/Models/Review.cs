namespace OnlineShop.Api.Models;

public class Review
{
    public string Id { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public int Rating { get; set; }          // 1–5
    public string Comment { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
