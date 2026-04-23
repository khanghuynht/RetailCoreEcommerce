using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.Domain.Abstractions;

namespace RetailCoreEcommerce.Persistence;

public sealed class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    private readonly DbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> FindByIdAsync(TKey id, bool tracking = true,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        query = includes.Aggregate(query, (current, include) => current.Include(include));

        return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    public async Task<TEntity?> FindSingleAsync(Expression<Func<TEntity, bool>>? predicate = null, bool tracking = true,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        query = includes.Aggregate(query, (current, include) => current.Include(include));

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>>? predicate = null, bool tracking = true,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        query = includes.Aggregate(query, (current, include) => current.Include(include));

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        query = includes.Aggregate(query, (current, include) => current.Include(include));

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>>? predicate = null, bool tracking = false,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        query = includes.Aggregate(query, (current, include) => current.Include(include));

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return query;
    }


    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        query = includes.Aggregate(query, (current, include) => current.Include(include));

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync(cancellationToken);
    }


    public async Task<PaginationResult<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>>? predicate,
        PaginationParams pagination,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        var totalItems = await query.CountAsync(cancellationToken);

        query = orderBy is not null
            ? orderBy(query)
            : query.OrderBy(x => x.Id); // fallback stable order

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResult<TEntity>(items, totalItems, pagination.PageNumber, pagination.PageSize);
    }

    public void Add(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public void AddRange(IEnumerable<TEntity> entities)
    {
        _dbSet.AddRange(entities);
    }

    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public void DeleteRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public void Detach(TEntity entity)
    {
        _context.Entry(entity).State = EntityState.Detached;
    }
}