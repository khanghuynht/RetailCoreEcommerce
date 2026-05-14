namespace RetailCoreEcommerce.Contracts.Domain;

public interface IEntity<TKey>
{
    TKey Id { get; set; }
}