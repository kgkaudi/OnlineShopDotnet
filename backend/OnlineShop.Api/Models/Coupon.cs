namespace OnlineShop.Api.Models;

public class Coupon
{
    public string Id { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Type { get; set; } = null!; // "percentage" or "fixed"
    public decimal Value { get; set; }
    public DateTime Expiration { get; set; }
    public int MaxUsage { get; set; }
    public int UsedCount { get; set; }
    public bool Active { get; set; } = true;
}
