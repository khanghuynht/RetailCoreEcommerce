namespace RetailCoreEcommerce.Domain.Constants;

public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Paid = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
}