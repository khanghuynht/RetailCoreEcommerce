namespace RetailCoreEcommerce.Contracts.Models.Cart;

public class Cart
{
    public Guid UserId { get; set; }
    public List<CartItem> Items { get; set; } = [];
    public decimal Total => Items.Sum(i => i.SubTotal);
    public DateTime UpdatedAt { get; set; }
}
