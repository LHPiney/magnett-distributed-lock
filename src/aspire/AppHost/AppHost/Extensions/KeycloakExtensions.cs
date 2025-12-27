using Aspire.Hosting;
using Magnett.Locks.AppHost.Options;

namespace Magnett.Locks.AppHost.Extensions;

public static class KeycloakExtensions
{
    public static IResourceBuilder<ContainerResource> AddKeycloakContainer(
        this IDistributedApplicationBuilder builder,
        KeycloakOptions options,
        KeycloakCredentialsOptions credentials)
    {
        var keycloakAdmin = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN") ?? credentials.Admin;
        var keycloakAdminPassword = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD") ?? credentials.AdminPassword;

        return builder.AddContainer("keycloak", options.Image)
            .WithEndpoint(name: "keycloak-http", port: options.Port, targetPort: options.TargetPort)
            .WithBindMount("../infrastructure/keycloak/realm-export.json", "/opt/keycloak/data/import/realm-export.json", isReadOnly: true)
            .WithEnvironment("KEYCLOAK_ADMIN", keycloakAdmin)
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", keycloakAdminPassword)
            .WithEnvironment("KC_HEALTH_ENABLED", "true")
            .WithEnvironment("KC_METRICS_ENABLED", "true")
            .WithArgs("start-dev", "--import-realm", "--http-port", "8080");
    }

    public static string BuildAuthAuthority(KeycloakConfigOptions config)
    {
        return $"http://keycloak:8080/realms/{config.Realm}";
    }
}

