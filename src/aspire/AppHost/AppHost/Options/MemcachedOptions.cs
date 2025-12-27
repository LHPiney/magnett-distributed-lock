namespace Magnett.Locks.AppHost.Options;

public sealed class MemcachedOptions
{
    public const string SectionName = "Services:Memcached";
    
    public string Image { get; set; } = "memcached:1.6";
    public int Port { get; set; } = 11211;
}

