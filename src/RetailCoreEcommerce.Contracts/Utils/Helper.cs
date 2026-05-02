namespace RetailCoreEcommerce.Contracts.Utils;

public static class Helper
{
    public static string GetCartKey(Guid userId) => $"cart:{userId}";
}