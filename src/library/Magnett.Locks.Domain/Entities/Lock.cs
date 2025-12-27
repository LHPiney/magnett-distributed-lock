namespace Magnett.Locks.Domain.Entities;

public sealed class Lock
{
    public required string TenantId { get; init; }
    public required string Environment { get; init; }
    public required string Namespace { get; init; }
    public required string ResourceId { get; init; }
    public required string LockId { get; init; }
    public required string OwnerId { get; init; }
    public required DateTime ExpiresAt { get; set; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; set; }

    public bool IsExpired() => ExpiresAt <= DateTime.UtcNow;
    
    public bool CanBeRenewed() => !IsExpired();
    
    public void Renew(DateTime newExpiresAt)
    {
        if (IsExpired())
            throw new InvalidOperationException("Cannot renew an expired lock");
        
        ExpiresAt = newExpiresAt;
        UpdatedAt = DateTime.UtcNow;
    }
}

