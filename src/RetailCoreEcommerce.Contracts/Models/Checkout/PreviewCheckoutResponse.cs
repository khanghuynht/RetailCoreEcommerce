namespace RetailCoreEcommerce.Contracts.Models.Checkout;

public class PreviewCheckoutResponse
{
    public Guid UserId { get; set; }
    public List<PreviewCheckoutItemResponse> Items { get; set; } = [];

    public decimal Total => Items.Sum(i => i.SubTotal);

    /// <summary>True when any item is unavailable, has insufficient stock, or has a price change.</summary>
    public bool HasIssues => Items.Any(i => !i.IsAvailable || i.StockInsufficient || i.PriceChanged);
}
