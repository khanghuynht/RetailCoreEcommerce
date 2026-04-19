using RetailCoreEcommerce.Domain;
using RetailCoreEcommerce.Domain.Abstractions;

namespace RetailCoreEcommerce.Services.Abstractions;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Get repository for specific entity type
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <returns></returns>
    IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>;


    /// <summary>
    ///     Save changes
    /// </summary>
    /// <param name="trackAudit">Whether to track audit changes</param>
    /// <param name="trackSoftDelete">Whether to track soft delete changes, if false, will delete entity from database</param>
    /// <returns>Number of affected rows</returns>
    Task<int> SaveChangesAsync(bool trackAudit = true, bool trackSoftDelete = true);
}