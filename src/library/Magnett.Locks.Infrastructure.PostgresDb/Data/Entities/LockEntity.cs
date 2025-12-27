namespace Magnett.Locks.Infrastructure.PostgresDb.Data.Entities;

public sealed class LockEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string LockId { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

