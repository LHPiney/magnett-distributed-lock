namespace Magnett.Locks.Domain.ValueObjects;

public sealed record LockHandle
{
    public required string TenantId { get; init; }
    public required string Environment { get; init; }
    public required string Namespace { get; init; }
    public required string ResourceId { get; init; }
    public required string LockId { get; init; }
    public required string OwnerId { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required DateTime AcquiredAt { get; init; }
}

