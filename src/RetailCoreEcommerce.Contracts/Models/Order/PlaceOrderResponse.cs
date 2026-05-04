namespace RetailCoreEcommerce.Contracts.Models.Order;

public class PlaceOrderResponse
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string PublishableKey { get; set; } = null!;
    public decimal TotalAmount { get; set; }
}
