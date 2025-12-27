using Magnett.Locks.Domain.Entities;
using LockEntity = Magnett.Locks.Domain.Entities.Lock;
using Xunit;

namespace Magnett.Locks.Domain.Tests.Entities;

public sealed class LockTests
{
    [Fact]
    [Trait("Category", "UnitTest")]
    public void IsExpired_WhenExpiresAtIsInPast_ReturnsTrue()
    {
        var @lock = new LockEntity
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "lock1",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        Assert.True(@lock.IsExpired());
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void IsExpired_WhenExpiresAtIsInFuture_ReturnsFalse()
    {
        var @lock = new LockEntity
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "lock1",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Assert.False(@lock.IsExpired());
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void CanBeRenewed_WhenNotExpired_ReturnsTrue()
    {
        var @lock = new LockEntity
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "lock1",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Assert.True(@lock.CanBeRenewed());
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void CanBeRenewed_WhenExpired_ReturnsFalse()
    {
        var @lock = new LockEntity
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "lock1",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        Assert.False(@lock.CanBeRenewed());
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void Renew_WhenNotExpired_UpdatesExpiresAt()
    {
        var @lock = new LockEntity
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "lock1",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var newExpiresAt = DateTime.UtcNow.AddMinutes(10);
        @lock.Renew(newExpiresAt);

        Assert.Equal(newExpiresAt, @lock.ExpiresAt);
        Assert.True(@lock.UpdatedAt >= DateTime.UtcNow.AddSeconds(-1));
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public void Renew_WhenExpired_ThrowsInvalidOperationException()
    {
        var @lock = new LockEntity
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "lock1",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var newExpiresAt = DateTime.UtcNow.AddMinutes(10);
        Assert.Throws<InvalidOperationException>(() => @lock.Renew(newExpiresAt));
    }
}

