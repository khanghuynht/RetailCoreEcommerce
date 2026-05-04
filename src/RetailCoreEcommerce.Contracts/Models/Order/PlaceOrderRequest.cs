namespace RetailCoreEcommerce.Contracts.Models.Order;

public class PlaceOrderRequest
{
    public string RecipientName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string StreetAddress { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string Ward { get; set; } = null!;
    public string? Notes { get; set; }
    
}
