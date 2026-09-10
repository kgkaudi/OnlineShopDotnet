namespace OnlineShop.Api.Models;

public class InventoryEvent
{
    public string Id { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public int Change { get; set; }          // +10 restock, -1 sale
    public string Reason { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
