using Magnett.Locks.Domain.Entities;
using LockEntity = Magnett.Locks.Domain.Entities.Lock;

namespace Magnett.Locks.Domain.Repositories;

public interface ILockRepository
{
    Task<LockEntity?> GetLockAsync(string tenantId, string environment, string @namespace, string resourceId, CancellationToken cancellationToken = default);
    Task<LockEntity> CreateLockAsync(LockEntity @lock, CancellationToken cancellationToken = default);
    Task DeleteLockAsync(string tenantId, string environment, string @namespace, string resourceId, string lockId, CancellationToken cancellationToken = default);
    Task<bool> RenewLockAsync(string tenantId, string environment, string @namespace, string resourceId, string lockId, DateTime newExpiresAt, CancellationToken cancellationToken = default);
}

