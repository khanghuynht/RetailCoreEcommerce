namespace RetailCoreEcommerce.Contracts.Domain;

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
    bool IsDeleted { get; set; }
}