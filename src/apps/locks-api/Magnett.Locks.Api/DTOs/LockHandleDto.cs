namespace Magnett.Locks.Api.DTOs;

public readonly record struct LockHandleDto
{
    public required string TenantId { get; init; }
    public required string Environment { get; init; }
    public required string Namespace { get; init; }
    public required string ResourceId { get; init; }
    public required string LockId { get; init; }
    public required string OwnerId { get; init; }
    public required long ExpiresAtUnix { get; init; }
    public required long AcquiredAtUnix { get; init; }

    public global::Locks.Api.LockHandle ToProto()
    {
        return new global::Locks.Api.LockHandle
        {
            TenantId = TenantId,
            Environment = Environment,
            Namespace = Namespace,
            ResourceId = ResourceId,
            LockId = LockId,
            OwnerId = OwnerId,
            ExpiresAtUnix = ExpiresAtUnix,
            AcquiredAtUnix = AcquiredAtUnix
        };
    }

    public static LockHandleDto FromProto(global::Locks.Api.LockHandle proto)
    {
        return new LockHandleDto
        {
            TenantId = proto.TenantId,
            Environment = proto.Environment,
            Namespace = proto.Namespace,
            ResourceId = proto.ResourceId,
            LockId = proto.LockId,
            OwnerId = proto.OwnerId,
            ExpiresAtUnix = proto.ExpiresAtUnix,
            AcquiredAtUnix = proto.AcquiredAtUnix
        };
    }
}

