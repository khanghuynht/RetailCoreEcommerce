namespace RetailCoreEcommerce.Domain.Abstractions;

/// <summary>
///     Base entity class that implements basic entity properties
/// </summary>
/// <typeparam name="TKey">Type of the primary key</typeparam>
public abstract class BaseEntity<TKey> : IEntity<TKey>
{
    public TKey Id { get; set; } = default!;
}

/// <summary>
///     Auditable base entity class that implements both entity and auditable properties
/// </summary>
/// <typeparam name="TKey">Type of the primary key</typeparam>
public abstract class AuditableEntity<TKey> : BaseEntity<TKey>, IAuditable, ISoftDeletable
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}