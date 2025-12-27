using Magnett.Locks.Domain.ValueObjects;

namespace Magnett.Locks.Domain.Services;

public interface ILockService
{
    Task<LockHandle> TryAcquireAsync(LockRequest request, string ownerId, CancellationToken cancellationToken = default);
    Task ReleaseAsync(LockHandle handle, CancellationToken cancellationToken = default);
    Task<LockHandle> RenewAsync(LockHandle handle, TimeSpan ttl, CancellationToken cancellationToken = default);
}

