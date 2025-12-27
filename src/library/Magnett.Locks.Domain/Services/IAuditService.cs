namespace Magnett.Locks.Domain.Services;

public interface IAuditService
{
    Task RecordEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}

public sealed class AuditEvent
{
    public required string EventType { get; init; }
    public required string TenantId { get; init; }
    public required string Environment { get; init; }
    public required string Namespace { get; init; }
    public required string ResourceId { get; init; }
    public string? LockId { get; init; }
    public string? OwnerId { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string Outcome { get; init; }
    public string? ErrorMessage { get; init; }
}

