using Magnett.Locks.Api.DTOs;
using Magnett.Locks.Domain.Exceptions;
using Magnett.Locks.Domain.ValueObjects;
using LockHandleDomain = Magnett.Locks.Domain.ValueObjects.LockHandle;

namespace Magnett.Locks.Api.Mappers;

public static class LockMapper
{
    public static LockRequest ToDomain(TryAcquireRequestDto dto)
    {
        return new LockRequest
        {
            TenantId = dto.TenantId,
            Environment = dto.Environment,
            Namespace = dto.Namespace,
            ResourceId = dto.ResourceId,
            Ttl = TimeSpan.FromSeconds(dto.TtlSeconds),
            RequestId = dto.RequestId
        };
    }

    public static LockHandleDto ToDto(LockHandleDomain domain)
    {
        return new LockHandleDto
        {
            TenantId = domain.TenantId,
            Environment = domain.Environment,
            Namespace = domain.Namespace,
            ResourceId = domain.ResourceId,
            LockId = domain.LockId,
            OwnerId = domain.OwnerId,
            ExpiresAtUnix = ((DateTimeOffset)domain.ExpiresAt).ToUnixTimeSeconds(),
            AcquiredAtUnix = ((DateTimeOffset)domain.AcquiredAt).ToUnixTimeSeconds()
        };
    }

    public static LockHandleDomain FromDto(LockHandleDto dto)
    {
        return new LockHandleDomain
        {
            TenantId = dto.TenantId,
            Environment = dto.Environment,
            Namespace = dto.Namespace,
            ResourceId = dto.ResourceId,
            LockId = dto.LockId,
            OwnerId = dto.OwnerId,
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(dto.ExpiresAtUnix).DateTime,
            AcquiredAt = DateTimeOffset.FromUnixTimeSeconds(dto.AcquiredAtUnix).DateTime
        };
    }

    public static ErrorDto ToDto(LockException exception)
    {
        return exception switch
        {
            InvalidArgumentException => new ErrorDto { Code = "INVALID_ARGUMENT", Message = exception.Message },
            AlreadyLockedException => new ErrorDto { Code = "ALREADY_LOCKED", Message = exception.Message },
            LockNotFoundException => new ErrorDto { Code = "NOT_FOUND", Message = exception.Message },
            LockConflictException => new ErrorDto { Code = "CONFLICT", Message = exception.Message },
            BackendUnavailableException => new ErrorDto { Code = "BACKEND_UNAVAILABLE", Message = exception.Message },
            UnauthorizedException => new ErrorDto { Code = "UNAUTHORIZED", Message = exception.Message },
            _ => new ErrorDto { Code = "INTERNAL_ERROR", Message = "Internal server error" }
        };
    }
}

