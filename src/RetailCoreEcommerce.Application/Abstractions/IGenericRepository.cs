using System.Linq.Expressions;
using RetailCoreEcommerce.Domain.Abstractions;

namespace RetailCoreEcommerce.Services.Abstractions;

public interface IGenericRepository<TEntity, in TKey> where TEntity : class, IEntity<TKey>
{
    /// <summary>
    ///     Find by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="includes"></param>
    /// <returns></returns>
    Task<TEntity?> FindByIdAsync(TKey id, bool tracking = true, CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    ///     As tracking
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="includes"></param>
    /// <returns></returns>
    Task<TEntity?> FindSingleAsync(Expression<Func<TEntity, bool>>? predicate = null, bool tracking = true,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    ///     As tracking
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="includes"></param>
    /// <returns></returns>
    Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>>? predicate = null, bool tracking = true,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    ///     Check if any entity exists and return bool type for optimization
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="includes"></param>
    /// <returns></returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    ///     Find all
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="includes"></param>
    /// <returns></returns>
    IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>>? predicate = null, bool tracking = false,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);

    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);

    void Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);
    void UpdateRange(IEnumerable<TEntity> entities);
    void DeleteRange(IEnumerable<TEntity> entities);

    void Detach(TEntity entity);
}