using Magnett.Locks.Domain.Entities;
using LockEntity = Magnett.Locks.Domain.Entities.Lock;

namespace Magnett.Locks.Domain.Services;

public interface ICacheService
{
    Task<LockEntity?> GetLockAsync(string cacheKey, CancellationToken cancellationToken = default);
    Task SetLockAsync(string cacheKey, LockEntity @lock, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task InvalidateLockAsync(string cacheKey, CancellationToken cancellationToken = default);
}

