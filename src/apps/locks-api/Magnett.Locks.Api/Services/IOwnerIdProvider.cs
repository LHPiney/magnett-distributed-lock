namespace Magnett.Locks.Api.Services;

public interface IOwnerIdProvider
{
    string GetOwnerId();
}

public sealed class OwnerIdProvider : IOwnerIdProvider
{
    private readonly string _ownerId;

    public OwnerIdProvider()
    {
        _ownerId = $"{Environment.MachineName}-{Environment.ProcessId}-{Guid.NewGuid():N}";
    }

    public string GetOwnerId() => _ownerId;
}

