using Magnett.Locks.Domain.Entities;
using Magnett.Locks.Domain.Exceptions;
using Magnett.Locks.Domain.Repositories;
using Magnett.Locks.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using LockEntity = Magnett.Locks.Domain.Entities.Lock;

namespace Magnett.Locks.Domain.Services.Implementations;

public sealed class LockService : ILockService
{
    private readonly ILockRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly IAuditService _auditService;
    private readonly ILogger<LockService> _logger;
    private static readonly Dictionary<string, LockHandle> _idempotencyCache = new();
    private static readonly object _idempotencyLock = new();

    public LockService(
        ILockRepository repository,
        ICacheService cacheService,
        IAuditService auditService,
        ILogger<LockService> logger)
    {
        _repository = repository;
        _cacheService = cacheService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<LockHandle> TryAcquireAsync(LockRequest request, string ownerId, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        if (!string.IsNullOrWhiteSpace(request.RequestId))
        {
            var cached = GetCachedResult(request.RequestId);
            if (cached != null)
            {
                return cached;
            }
        }

        try
        {
            var existing = await _repository.GetLockAsync(
                request.TenantId,
                request.Environment,
                request.Namespace,
                request.ResourceId,
                cancellationToken);

            if (existing != null && !existing.IsExpired())
            {
                await RecordAuditEvent("lock.conflict", request, null, "failed", "Lock already exists and is not expired");
                throw new AlreadyLockedException($"Resource is locked by {existing.OwnerId} until {existing.ExpiresAt:O}");
            }

            var lockId = GenerateLockId();
            var now = DateTime.UtcNow;
            var expiresAt = now.Add(request.Ttl);

            var newLock = new LockEntity
            {
                TenantId = request.TenantId,
                Environment = request.Environment,
                Namespace = request.Namespace,
                ResourceId = request.ResourceId,
                LockId = lockId,
                OwnerId = ownerId,
                ExpiresAt = expiresAt,
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await _repository.CreateLockAsync(newLock, cancellationToken);

            var handle = new LockHandle
            {
                TenantId = created.TenantId,
                Environment = created.Environment,
                Namespace = created.Namespace,
                ResourceId = created.ResourceId,
                LockId = created.LockId,
                OwnerId = created.OwnerId,
                ExpiresAt = created.ExpiresAt,
                AcquiredAt = created.CreatedAt
            };

            var cacheKey = BuildCacheKey(request.TenantId, request.Environment, request.Namespace, request.ResourceId);
            await _cacheService.SetLockAsync(cacheKey, created, request.Ttl, cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.RequestId))
            {
                CacheResult(request.RequestId, handle);
            }

            await RecordAuditEvent("lock.acquire", request, lockId, ownerId, "success");

            return handle;
        }
        catch (AlreadyLockedException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not LockException)
        {
            _logger.LogError(ex, "Failed to acquire lock for resource {ResourceId}", request.ResourceId);
            await RecordAuditEvent("error.acquire", request, null, null, "failed", ex.Message);
            throw new BackendUnavailableException("Failed to acquire lock due to backend error");
        }
    }

    public async Task ReleaseAsync(LockHandle handle, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _repository.GetLockAsync(
                handle.TenantId,
                handle.Environment,
                handle.Namespace,
                handle.ResourceId,
                cancellationToken);

            if (existing == null)
            {
                await RecordAuditEvent("lock.release", handle, "failed", "Lock not found");
                throw new LockNotFoundException();
            }

            if (existing.LockId != handle.LockId)
            {
                await RecordAuditEvent("lock.conflict", handle, "failed", $"Lock ID mismatch: expected {existing.LockId}, got {handle.LockId}");
                throw new LockConflictException($"Lock ID mismatch: expected {existing.LockId}, got {handle.LockId}");
            }

            await _repository.DeleteLockAsync(
                handle.TenantId,
                handle.Environment,
                handle.Namespace,
                handle.ResourceId,
                handle.LockId,
                cancellationToken);

            var cacheKey = BuildCacheKey(handle.TenantId, handle.Environment, handle.Namespace, handle.ResourceId);
            await _cacheService.InvalidateLockAsync(cacheKey, cancellationToken);

            await RecordAuditEvent("lock.release", handle, "success");
        }
        catch (LockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release lock {LockId}", handle.LockId);
            await RecordAuditEvent("error.release", handle, "failed", ex.Message);
            throw new BackendUnavailableException("Failed to release lock due to backend error");
        }
    }

    public async Task<LockHandle> RenewAsync(LockHandle handle, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _repository.GetLockAsync(
                handle.TenantId,
                handle.Environment,
                handle.Namespace,
                handle.ResourceId,
                cancellationToken);

            if (existing == null)
            {
                await RecordAuditEvent("lock.renew", handle, "failed", "Lock not found");
                throw new LockNotFoundException();
            }

            if (existing.LockId != handle.LockId)
            {
                await RecordAuditEvent("lock.conflict", handle, "failed", $"Lock ID mismatch: expected {existing.LockId}, got {handle.LockId}");
                throw new LockConflictException($"Lock ID mismatch: expected {existing.LockId}, got {handle.LockId}");
            }

            if (!existing.CanBeRenewed())
            {
                await RecordAuditEvent("lock.expire", handle, "failed", "Lock has expired");
                throw new LockNotFoundException("Lock has expired");
            }

            var newExpiresAt = DateTime.UtcNow.Add(ttl);
            var renewed = await _repository.RenewLockAsync(
                handle.TenantId,
                handle.Environment,
                handle.Namespace,
                handle.ResourceId,
                handle.LockId,
                newExpiresAt,
                cancellationToken);

            if (!renewed)
            {
                await RecordAuditEvent("lock.renew", handle, "failed", "Failed to renew lock");
                throw new LockNotFoundException("Failed to renew lock");
            }

            existing.Renew(newExpiresAt);

            var updatedHandle = new LockHandle
            {
                TenantId = handle.TenantId,
                Environment = handle.Environment,
                Namespace = handle.Namespace,
                ResourceId = handle.ResourceId,
                LockId = handle.LockId,
                OwnerId = handle.OwnerId,
                ExpiresAt = newExpiresAt,
                AcquiredAt = handle.AcquiredAt
            };

            var cacheKey = BuildCacheKey(handle.TenantId, handle.Environment, handle.Namespace, handle.ResourceId);
            await _cacheService.SetLockAsync(cacheKey, existing, ttl, cancellationToken);

            await RecordAuditEvent("lock.renew", handle, "success");

            return updatedHandle;
        }
        catch (LockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to renew lock {LockId}", handle.LockId);
            await RecordAuditEvent("error.renew", handle, "failed", ex.Message);
            throw new BackendUnavailableException("Failed to renew lock due to backend error");
        }
    }

    private static void ValidateRequest(LockRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TenantId))
            throw new InvalidArgumentException("TenantId is required");
        if (string.IsNullOrWhiteSpace(request.Environment))
            throw new InvalidArgumentException("Environment is required");
        if (string.IsNullOrWhiteSpace(request.Namespace))
            throw new InvalidArgumentException("Namespace is required");
        if (string.IsNullOrWhiteSpace(request.ResourceId))
            throw new InvalidArgumentException("ResourceId is required");
        if (request.Ttl <= TimeSpan.Zero)
            throw new InvalidArgumentException("TTL must be greater than zero");
    }

    private static string GenerateLockId()
    {
        return Guid.NewGuid().ToString("N");
    }

    private static string BuildCacheKey(string tenantId, string environment, string @namespace, string resourceId)
    {
        return $"lock:{tenantId}:{environment}:{@namespace}:{resourceId}";
    }

    private static LockHandle? GetCachedResult(string requestId)
    {
        lock (_idempotencyLock)
        {
            return _idempotencyCache.TryGetValue(requestId, out var handle) ? handle : null;
        }
    }

    private static void CacheResult(string requestId, LockHandle handle)
    {
        lock (_idempotencyLock)
        {
            if (_idempotencyCache.Count > 10000)
            {
                _idempotencyCache.Clear();
            }
            _idempotencyCache[requestId] = handle;
        }
    }

    private async Task RecordAuditEvent(string eventType, LockRequest request, string? lockId, string? ownerId, string outcome, string? errorMessage = null)
    {
        var auditEvent = new AuditEvent
        {
            EventType = eventType,
            TenantId = request.TenantId,
            Environment = request.Environment,
            Namespace = request.Namespace,
            ResourceId = request.ResourceId,
            LockId = lockId,
            OwnerId = ownerId,
            Timestamp = DateTime.UtcNow,
            Outcome = outcome,
            ErrorMessage = errorMessage
        };
        await _auditService.RecordEventAsync(auditEvent);
    }

    private async Task RecordAuditEvent(string eventType, LockHandle handle, string outcome, string? errorMessage = null)
    {
        var auditEvent = new AuditEvent
        {
            EventType = eventType,
            TenantId = handle.TenantId,
            Environment = handle.Environment,
            Namespace = handle.Namespace,
            ResourceId = handle.ResourceId,
            LockId = handle.LockId,
            OwnerId = handle.OwnerId,
            Timestamp = DateTime.UtcNow,
            Outcome = outcome,
            ErrorMessage = errorMessage
        };
        await _auditService.RecordEventAsync(auditEvent);
    }
}

