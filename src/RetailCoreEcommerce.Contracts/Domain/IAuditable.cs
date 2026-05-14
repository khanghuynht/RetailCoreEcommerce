namespace RetailCoreEcommerce.Contracts.Domain;

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}