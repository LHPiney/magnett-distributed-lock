namespace Magnett.Locks.Api.Options;

public sealed class KestrelOptions
{
    public const string SectionName = "Kestrel";
    
    public int HttpPort { get; set; } = 8080;
    public int GrpcPort { get; set; } = 8081;
}

