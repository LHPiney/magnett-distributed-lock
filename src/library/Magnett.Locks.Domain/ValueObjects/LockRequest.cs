namespace Magnett.Locks.Domain.ValueObjects;

public sealed class LockRequest
{
    public required string TenantId { get; init; }
    public required string Environment { get; init; }
    public required string Namespace { get; init; }
    public required string ResourceId { get; init; }
    public required TimeSpan Ttl { get; init; }
    public string? RequestId { get; init; }
}

