using Aspire.Hosting;
using Magnett.Locks.AppHost.Options;

namespace Magnett.Locks.AppHost.Extensions;

public static class MemcachedExtensions
{
    public static IResourceBuilder<ContainerResource> AddMemcachedContainer(
        this IDistributedApplicationBuilder builder,
        MemcachedOptions options)
    {
        return builder.AddContainer("memcached", options.Image)
            .WithEndpoint(name: "memcached", port: options.Port, targetPort: 11211);
    }
}

