using Magnett.Locks.Domain.Entities;
using Magnett.Locks.Domain.Exceptions;
using Magnett.Locks.Domain.Repositories;
using Magnett.Locks.Infrastructure.PostgresDb.Data;
using Magnett.Locks.Infrastructure.PostgresDb.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using LockEntity = Magnett.Locks.Domain.Entities.Lock;

namespace Magnett.Locks.Infrastructure.PostgresDb.Repositories;

public sealed class LockRepository : ILockRepository
{
    private readonly LockDbContext _context;
    private readonly ILogger<LockRepository> _logger;

    public LockRepository(LockDbContext context, ILogger<LockRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LockEntity?> GetLockAsync(string tenantId, string environment, string @namespace, string resourceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.Locks
                .FromSqlRaw(@"
                    SELECT tenant_id, environment, namespace, resource_id, lock_id, owner_id, expires_at, created_at, updated_at
                    FROM locks
                    WHERE tenant_id = {0}
                      AND environment = {1}
                      AND namespace = {2}
                      AND resource_id = {3}
                    FOR UPDATE
                ", tenantId, environment, @namespace, resourceId)
                .FirstOrDefaultAsync(cancellationToken);

            return entity != null ? LockMapper.ToDomain(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get lock for resource {ResourceId}", resourceId);
            throw new BackendUnavailableException("Failed to retrieve lock from database", ex);
        }
    }

    public async Task<LockEntity> CreateLockAsync(LockEntity @lock, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = LockMapper.ToEntity(@lock);
            _context.Locks.Add(entity);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return LockMapper.ToDomain(entity);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                throw new AlreadyLockedException();
            }
        }
        catch (AlreadyLockedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create lock for resource {ResourceId}", @lock.ResourceId);
            throw new BackendUnavailableException("Failed to create lock in database", ex);
        }
    }

    public async Task DeleteLockAsync(string tenantId, string environment, string @namespace, string resourceId, string lockId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.Locks
                .FirstOrDefaultAsync(l =>
                    l.TenantId == tenantId &&
                    l.Environment == environment &&
                    l.Namespace == @namespace &&
                    l.ResourceId == resourceId &&
                    l.LockId == lockId,
                    cancellationToken);

            if (entity == null)
            {
                throw new LockNotFoundException();
            }

            _context.Locks.Remove(entity);
            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                throw new LockNotFoundException();
            }
        }
        catch (LockNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete lock {LockId}", lockId);
            throw new BackendUnavailableException("Failed to delete lock from database", ex);
        }
    }

    public async Task<bool> RenewLockAsync(string tenantId, string environment, string @namespace, string resourceId, string lockId, DateTime newExpiresAt, CancellationToken cancellationToken = default)
    {
        try
        {
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(@"
                UPDATE locks
                SET expires_at = {0}, updated_at = NOW()
                WHERE tenant_id = {1}
                  AND environment = {2}
                  AND namespace = {3}
                  AND resource_id = {4}
                  AND lock_id = {5}
                  AND expires_at > NOW()
            ", newExpiresAt, tenantId, environment, @namespace, resourceId, lockId);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to renew lock {LockId}", lockId);
            throw new BackendUnavailableException("Failed to renew lock in database", ex);
        }
    }
}

