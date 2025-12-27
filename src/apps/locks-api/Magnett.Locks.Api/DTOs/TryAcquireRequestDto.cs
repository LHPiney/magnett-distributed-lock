namespace Magnett.Locks.Api.DTOs;

public readonly record struct TryAcquireRequestDto
{
    public required string TenantId { get; init; }
    public required string Environment { get; init; }
    public required string Namespace { get; init; }
    public required string ResourceId { get; init; }
    public required long TtlSeconds { get; init; }
    public string? RequestId { get; init; }

    public static TryAcquireRequestDto FromProto(global::Locks.Api.TryAcquireRequest proto)
    {
        return new TryAcquireRequestDto
        {
            TenantId = proto.TenantId,
            Environment = proto.Environment,
            Namespace = proto.Namespace,
            ResourceId = proto.ResourceId,
            TtlSeconds = proto.TtlSeconds,
            RequestId = string.IsNullOrWhiteSpace(proto.RequestId) ? null : proto.RequestId
        };
    }
}

