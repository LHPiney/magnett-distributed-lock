using Magnett.Locks.Domain.Entities;
using Magnett.Locks.Infrastructure.PostgresDb.Data.Entities;
using LockEntity = Magnett.Locks.Domain.Entities.Lock;

namespace Magnett.Locks.Infrastructure.PostgresDb.Mappers;

public static class LockMapper
{
    public static Data.Entities.LockEntity ToEntity(LockEntity domain)
    {
        return new Data.Entities.LockEntity
        {
            TenantId = domain.TenantId,
            Environment = domain.Environment,
            Namespace = domain.Namespace,
            ResourceId = domain.ResourceId,
            LockId = domain.LockId,
            OwnerId = domain.OwnerId,
            ExpiresAt = domain.ExpiresAt,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }

    public static LockEntity ToDomain(Data.Entities.LockEntity entity)
    {
        return new LockEntity
        {
            TenantId = entity.TenantId,
            Environment = entity.Environment,
            Namespace = entity.Namespace,
            ResourceId = entity.ResourceId,
            LockId = entity.LockId,
            OwnerId = entity.OwnerId,
            ExpiresAt = entity.ExpiresAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}

