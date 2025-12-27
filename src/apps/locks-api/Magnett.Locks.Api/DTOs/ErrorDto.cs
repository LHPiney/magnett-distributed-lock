namespace Magnett.Locks.Api.DTOs;

public readonly record struct ErrorDto
{
    public required string Code { get; init; }
    public required string Message { get; init; }

    public global::Locks.Api.Error ToProto()
    {
        return new global::Locks.Api.Error
        {
            Code = Code,
            Message = Message
        };
    }
}

