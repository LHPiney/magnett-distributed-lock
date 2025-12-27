using Grpc.Core;
using Magnett.Locks.Api.DTOs;
using Magnett.Locks.Api.Mappers;
using Magnett.Locks.Domain.Exceptions;
using Magnett.Locks.Domain.Services;
using global::Locks.Api;

namespace Magnett.Locks.Api.Services;

public class LockServiceEndpoint : global::Locks.Api.LockService.LockServiceBase
{
    private readonly ILockService _lockService;
    private readonly IOwnerIdProvider _ownerIdProvider;
    private readonly ILogger<LockServiceEndpoint> _logger;

    public LockServiceEndpoint(ILockService lockService, IOwnerIdProvider ownerIdProvider, ILogger<LockServiceEndpoint> logger)
    {
        _lockService = lockService;
        _ownerIdProvider = ownerIdProvider;
        _logger = logger;
    }

    public override Task<PingReply> Ping(PingRequest request, ServerCallContext context)
    {
        var message = string.IsNullOrWhiteSpace(request.Message) ? "pong" : request.Message;
        return Task.FromResult(new PingReply { Message = message });
    }

    public override async Task<TryAcquireReply> TryAcquire(TryAcquireRequest request, ServerCallContext context)
    {
        try
        {
            var dto = TryAcquireRequestDto.FromProto(request);
            var domainRequest = LockMapper.ToDomain(dto);
            var ownerId = _ownerIdProvider.GetOwnerId();
            var handle = await _lockService.TryAcquireAsync(domainRequest, ownerId, context.CancellationToken);
            var handleDto = LockMapper.ToDto(handle);

            return new TryAcquireReply
            {
                Handle = handleDto.ToProto()
            };
        }
        catch (LockException ex)
        {
            var errorDto = LockMapper.ToDto(ex);
            return new TryAcquireReply
            {
                Error = errorDto.ToProto()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in TryAcquire");
            return new TryAcquireReply
            {
                Error = new Error { Code = "INTERNAL_ERROR", Message = "Internal server error" }
            };
        }
    }

    public override async Task<ReleaseReply> Release(ReleaseRequest request, ServerCallContext context)
    {
        try
        {
            var dto = LockHandleDto.FromProto(request.Handle);
            var handle = LockMapper.FromDto(dto);
            await _lockService.ReleaseAsync(handle, context.CancellationToken);

            return new ReleaseReply
            {
                Success = new Success { Ok = true }
            };
        }
        catch (LockException ex)
        {
            var errorDto = LockMapper.ToDto(ex);
            return new ReleaseReply
            {
                Error = errorDto.ToProto()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Release");
            return new ReleaseReply
            {
                Error = new Error { Code = "INTERNAL_ERROR", Message = "Internal server error" }
            };
        }
    }

    public override async Task<RenewReply> Renew(RenewRequest request, ServerCallContext context)
    {
        try
        {
            var dto = LockHandleDto.FromProto(request.Handle);
            var handle = LockMapper.FromDto(dto);
            var renewedHandle = await _lockService.RenewAsync(handle, TimeSpan.FromSeconds(request.TtlSeconds), context.CancellationToken);
            var renewedDto = LockMapper.ToDto(renewedHandle);

            return new RenewReply
            {
                Handle = renewedDto.ToProto()
            };
        }
        catch (LockException ex)
        {
            var errorDto = LockMapper.ToDto(ex);
            return new RenewReply
            {
                Error = errorDto.ToProto()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Renew");
            return new RenewReply
            {
                Error = new Error { Code = "INTERNAL_ERROR", Message = "Internal server error" }
            };
        }
    }
}

