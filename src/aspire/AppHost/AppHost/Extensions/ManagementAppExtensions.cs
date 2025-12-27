using Aspire.Hosting;
using Magnett.Locks.AppHost.Options;

namespace Magnett.Locks.AppHost.Extensions;

public static class ManagementAppExtensions
{
    public static IResourceBuilder<ContainerResource> AddManagementAppContainer(
        this IDistributedApplicationBuilder builder,
        ManagementAppOptions options,
        KeycloakConfigOptions keycloakConfig)
    {
        return builder.AddContainer("management-app", options.Image)
            .WithEndpoint(name: "management-app", port: options.Port, targetPort: options.TargetPort)
            .WithEnvironment("NG_APP_OIDC_ISSUER", $"http://keycloak:8080/realms/{keycloakConfig.Realm}")
            .WithEnvironment("NG_APP_OIDC_CLIENT_ID", keycloakConfig.ClientId)
            .WithEnvironment("NG_APP_API_BASE_URL", "http://locks-api:8080");
    }
}

