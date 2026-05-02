namespace RetailCoreEcommerce.Contracts.Models.Cart;

public class UpdateCartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
