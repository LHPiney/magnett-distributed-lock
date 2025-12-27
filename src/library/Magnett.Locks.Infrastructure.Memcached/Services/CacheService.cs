using System.Text.Json;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using Magnett.Locks.Domain.Entities;
using Magnett.Locks.Domain.Services;
using Microsoft.Extensions.Logging;
using LockEntity = Magnett.Locks.Domain.Entities.Lock;

namespace Magnett.Locks.Infrastructure.Memcached.Services;

public sealed class CacheService : ICacheService
{
    private readonly IMemcachedClient _memcachedClient;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IMemcachedClient memcachedClient, ILogger<CacheService> logger)
    {
        _memcachedClient = memcachedClient;
        _logger = logger;
    }

    public async Task<LockEntity?> GetLockAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await _memcachedClient.GetAsync<string>(cacheKey);
            if (cached == null || !cached.HasValue)
            {
                return null;
            }

            var dto = JsonSerializer.Deserialize<CacheLockDto>(cached.Value);
            if (dto != null && dto.ExpiresAt > DateTime.UtcNow)
            {
                return new LockEntity
                {
                    TenantId = dto.TenantId,
                    Environment = dto.Environment,
                    Namespace = dto.Namespace,
                    ResourceId = dto.ResourceId,
                    LockId = dto.LockId,
                    OwnerId = dto.OwnerId,
                    ExpiresAt = dto.ExpiresAt,
                    CreatedAt = dto.CreatedAt,
                    UpdatedAt = dto.UpdatedAt
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for key {CacheKey}, ignoring", cacheKey);
            return null;
        }
    }

    public async Task SetLockAsync(string cacheKey, LockEntity @lock, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = new CacheLockDto
            {
                TenantId = @lock.TenantId,
                Environment = @lock.Environment,
                Namespace = @lock.Namespace,
                ResourceId = @lock.ResourceId,
                LockId = @lock.LockId,
                OwnerId = @lock.OwnerId,
                ExpiresAt = @lock.ExpiresAt,
                CreatedAt = @lock.CreatedAt,
                UpdatedAt = @lock.UpdatedAt
            };
            var json = JsonSerializer.Serialize(dto);
            var cacheTtlSeconds = (int)Math.Min(ttl.TotalSeconds, 3600);
            await _memcachedClient.SetAsync(cacheKey, json, cacheTtlSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache write failed for key {CacheKey}, ignoring", cacheKey);
        }
    }

    public async Task InvalidateLockAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _memcachedClient.RemoveAsync(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache invalidation failed for key {CacheKey}, ignoring", cacheKey);
        }
    }

    private sealed class CacheLockDto
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
}

