namespace Magnett.Locks.Infrastructure.Memcached.Options;

public sealed class MemcachedOptions
{
    public const string SectionName = "Memcached";
    
    public string Endpoint { get; set; } = string.Empty;
}

