namespace RetailCoreEcommerce.Contracts.Models.Cart;

public class CartItem
{
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal => UnitPrice * Quantity;
}
