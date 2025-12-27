using Magnett.Locks.Domain.Entities;
using Magnett.Locks.Domain.Exceptions;
using Magnett.Locks.Domain.Repositories;
using Magnett.Locks.Domain.Services;
using Magnett.Locks.Domain.Services.Implementations;
using Magnett.Locks.Domain.ValueObjects;
using LockEntity = Magnett.Locks.Domain.Entities.Lock;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Magnett.Locks.Domain.Tests.Services;

public sealed class LockServiceTests
{
    private readonly Mock<ILockRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ILogger<LockService>> _loggerMock;
    private readonly LockService _lockService;

    public LockServiceTests()
    {
        _repositoryMock = new Mock<ILockRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _auditServiceMock = new Mock<IAuditService>();
        _loggerMock = new Mock<ILogger<LockService>>();

        _lockService = new LockService(
            _repositoryMock.Object,
            _cacheServiceMock.Object,
            _auditServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public async Task TryAcquireAsync_WithValidRequest_ReturnsLockHandle()
    {
        var request = new LockRequest
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            Ttl = TimeSpan.FromMinutes(5)
        };

        _repositoryMock
            .Setup(r => r.GetLockAsync("tenant1", "prod", "ns1", "resource1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((LockEntity?)null);

        _repositoryMock
            .Setup(r => r.CreateLockAsync(It.IsAny<LockEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LockEntity lockEntity, CancellationToken _) => lockEntity);

        var handle = await _lockService.TryAcquireAsync(request, "owner1");

        Assert.NotNull(handle);
        Assert.Equal("tenant1", handle.TenantId);
        Assert.Equal("prod", handle.Environment);
        Assert.Equal("ns1", handle.Namespace);
        Assert.Equal("resource1", handle.ResourceId);
        Assert.Equal("owner1", handle.OwnerId);
        Assert.NotNull(handle.LockId);
        Assert.True(handle.ExpiresAt > DateTime.UtcNow);

        _repositoryMock.Verify(r => r.GetLockAsync("tenant1", "prod", "ns1", "resource1", It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.CreateLockAsync(It.IsAny<LockEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.SetLockAsync(It.IsAny<string>(), It.IsAny<LockEntity>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditServiceMock.Verify(a => a.RecordEventAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public async Task TryAcquireAsync_WhenLockAlreadyExists_ThrowsAlreadyLockedException()
    {
        var request = new LockRequest
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            Ttl = TimeSpan.FromMinutes(5)
        };

        var existingLock = new LockEntity
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "existing-lock",
            OwnerId = "owner2",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _repositoryMock
            .Setup(r => r.GetLockAsync("tenant1", "prod", "ns1", "resource1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLock);

        await Assert.ThrowsAsync<AlreadyLockedException>(() => _lockService.TryAcquireAsync(request, "owner1"));

        _repositoryMock.Verify(r => r.CreateLockAsync(It.IsAny<LockEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _auditServiceMock.Verify(a => a.RecordEventAsync(It.Is<AuditEvent>(e => e.EventType == "lock.conflict"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public async Task TryAcquireAsync_WhenRequestIsInvalid_ThrowsInvalidArgumentException()
    {
        var request = new LockRequest
        {
            TenantId = "",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            Ttl = TimeSpan.FromMinutes(5)
        };

        await Assert.ThrowsAsync<InvalidArgumentException>(() => _lockService.TryAcquireAsync(request, "owner1"));

        _repositoryMock.Verify(r => r.GetLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public async Task TryAcquireAsync_WhenTtlIsZero_ThrowsInvalidArgumentException()
    {
        var request = new LockRequest
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            Ttl = TimeSpan.Zero
        };

        await Assert.ThrowsAsync<InvalidArgumentException>(() => _lockService.TryAcquireAsync(request, "owner1"));
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public async Task ReleaseAsync_WithValidHandle_ReleasesLock()
    {
        var handle = new LockHandle
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "lock1",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            AcquiredAt = DateTime.UtcNow
        };

        var existingLock = new LockEntity
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

        _repositoryMock
            .Setup(r => r.GetLockAsync("tenant1", "prod", "ns1", "resource1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLock);

        await _lockService.ReleaseAsync(handle);

        _repositoryMock.Verify(r => r.DeleteLockAsync("tenant1", "prod", "ns1", "resource1", "lock1", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.InvalidateLockAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditServiceMock.Verify(a => a.RecordEventAsync(It.Is<AuditEvent>(e => e.EventType == "lock.release" && e.Outcome == "success"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public async Task ReleaseAsync_WhenLockNotFound_ThrowsLockNotFoundException()
    {
        var handle = new LockHandle
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "lock1",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            AcquiredAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetLockAsync("tenant1", "prod", "ns1", "resource1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((LockEntity?)null);

        await Assert.ThrowsAsync<LockNotFoundException>(() => _lockService.ReleaseAsync(handle));

        _repositoryMock.Verify(r => r.DeleteLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public async Task ReleaseAsync_WhenLockIdMismatch_ThrowsLockConflictException()
    {
        var handle = new LockHandle
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "lock1",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            AcquiredAt = DateTime.UtcNow
        };

        var existingLock = new LockEntity
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "different-lock",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetLockAsync("tenant1", "prod", "ns1", "resource1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLock);

        await Assert.ThrowsAsync<LockConflictException>(() => _lockService.ReleaseAsync(handle));
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public async Task RenewAsync_WithValidHandle_ReturnsRenewedHandle()
    {
        var handle = new LockHandle
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "lock1",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            AcquiredAt = DateTime.UtcNow
        };

        var existingLock = new LockEntity
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

        _repositoryMock
            .Setup(r => r.GetLockAsync("tenant1", "prod", "ns1", "resource1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLock);

        _repositoryMock
            .Setup(r => r.RenewLockAsync("tenant1", "prod", "ns1", "resource1", "lock1", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var renewedHandle = await _lockService.RenewAsync(handle, TimeSpan.FromMinutes(10));

        Assert.NotNull(renewedHandle);
        Assert.Equal(handle.LockId, renewedHandle.LockId);
        Assert.True(renewedHandle.ExpiresAt > handle.ExpiresAt);
        Assert.Equal(handle.AcquiredAt, renewedHandle.AcquiredAt);

        _repositoryMock.Verify(r => r.RenewLockAsync("tenant1", "prod", "ns1", "resource1", "lock1", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.SetLockAsync(It.IsAny<string>(), It.IsAny<LockEntity>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditServiceMock.Verify(a => a.RecordEventAsync(It.Is<AuditEvent>(e => e.EventType == "lock.renew" && e.Outcome == "success"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "UnitTest")]
    public async Task RenewAsync_WhenLockExpired_ThrowsLockNotFoundException()
    {
        var handle = new LockHandle
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "lock1",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            AcquiredAt = DateTime.UtcNow.AddMinutes(-10)
        };

        var existingLock = new LockEntity
        {
            TenantId = "tenant1",
            Environment = "prod",
            Namespace = "ns1",
            ResourceId = "resource1",
            LockId = "lock1",
            OwnerId = "owner1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _repositoryMock
            .Setup(r => r.GetLockAsync("tenant1", "prod", "ns1", "resource1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLock);

        await Assert.ThrowsAsync<LockNotFoundException>(() => _lockService.RenewAsync(handle, TimeSpan.FromMinutes(10)));
    }
}

