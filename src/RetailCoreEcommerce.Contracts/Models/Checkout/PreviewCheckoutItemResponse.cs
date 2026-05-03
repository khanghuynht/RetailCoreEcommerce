namespace RetailCoreEcommerce.Contracts.Models.Checkout;

public class PreviewCheckoutItemResponse
{
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }

    /// <summary>Cached price when item was added to cart.</summary>
    public decimal CachedUnitPrice { get; set; }

    /// <summary>Current effective price from the database (SalePrice ?? OriginalPrice).</summary>
    public decimal CurrentUnitPrice { get; set; }

    public int Quantity { get; set; }
    public decimal SubTotal => CurrentUnitPrice * Quantity;

    /// <summary>True when the current price differs from the cached cart price.</summary>
    public bool PriceChanged => CachedUnitPrice != CurrentUnitPrice;

    /// <summary>False when the product no longer exists or has been deactivated.</summary>
    public bool IsAvailable { get; set; }

    /// <summary>Remaining stock after subtracting reserved quantity.</summary>
    public int AvailableStock { get; set; }

    /// <summary>True when the requested quantity exceeds available stock.</summary>
    public bool StockInsufficient => IsAvailable && Quantity > AvailableStock;
}
