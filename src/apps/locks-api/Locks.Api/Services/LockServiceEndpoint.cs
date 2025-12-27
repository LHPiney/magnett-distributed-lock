using Grpc.Core;

namespace Locks.Api.Services;

public class LockServiceEndpoint : LockService.LockServiceBase
{
    /// <summary>
    /// Responds with a simple pong message for connectivity validation.
    /// </summary>
    /// <param name="request">Inbound ping request.</param>
    /// <param name="context">Call context.</param>
    /// <returns>Ping reply with echoed or default message.</returns>
    public override Task<PingReply> Ping(PingRequest request, ServerCallContext context)
    {
        var message = string.IsNullOrWhiteSpace(request.Message) ? "pong" : request.Message;
        return Task.FromResult(new PingReply { Message = message });
    }
}

