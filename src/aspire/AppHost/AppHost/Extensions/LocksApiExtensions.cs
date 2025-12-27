using Aspire.Hosting;
using Magnett.Locks.AppHost.Options;
using Projects;

namespace Magnett.Locks.AppHost.Extensions;

public static class LocksApiExtensions
{
    public static IResourceBuilder<ProjectResource> AddLocksApiProject(
        this IDistributedApplicationBuilder builder,
        string postgresConnection,
        (string User, string Password) rabbitMqCredentials,
        KeycloakConfigOptions keycloakConfig)
    {
        return builder.AddProject<Magnett_Locks_Api>("locks-api")
            .WithEnvironment("POSTGRES_CONNECTION", postgresConnection)
            .WithEnvironment("RABBITMQ_HOST", "rabbitmq")
            .WithEnvironment("RABBITMQ_USER", rabbitMqCredentials.User)
            .WithEnvironment("RABBITMQ_PASSWORD", rabbitMqCredentials.Password)
            .WithEnvironment("MEMCACHED_ENDPOINT", "memcached:11211")
            .WithEnvironment("AUTH_AUTHORITY", $"http://keycloak:8080/realms/{keycloakConfig.Realm}")
            .WithEnvironment("AUTH_AUDIENCE", "locks-api");
    }
}

