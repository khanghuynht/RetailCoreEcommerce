using Microsoft.EntityFrameworkCore;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Domain.Abstractions;
using RetailCoreEcommerce.Persistence.Data;
using RetailCoreEcommerce.Services.Abstractions;

namespace RetailCoreEcommerce.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;


    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class, IEntity<TKey> =>
        new GenericRepository<TEntity, TKey>(_context);


    public async Task<int> SaveChangesAsync(bool trackAudit = true, bool trackSoftDelete = true)
    {
        if (trackAudit)
        {
            ApplyAuditInfo();
        }

        if (trackSoftDelete)
        {
            ApplySoftDelete();
        }

        return await _context.SaveChangesAsync();
    }

    private void ApplyAuditInfo()
    {
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e is { Entity: IAuditable, State: EntityState.Added or EntityState.Modified });

        foreach (var entry in entries)
        {
            var entity = (IAuditable)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    private void ApplySoftDelete()
    {
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e is { Entity: ISoftDeletable, State: EntityState.Deleted });

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;
            var entity = (ISoftDeletable)entry.Entity;
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}