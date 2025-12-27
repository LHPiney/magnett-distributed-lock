using Aspire.Hosting;
using Magnett.Locks.AppHost.Options;

namespace Magnett.Locks.AppHost.Extensions;

public static class PostgresExtensions
{
    public static IResourceBuilder<ContainerResource> AddPostgresContainer(
        this IDistributedApplicationBuilder builder,
        PostgresOptions options,
        PostgresCredentialsOptions credentials)
    {
        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? credentials.User;
        var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? credentials.Password;
        var postgresDb = options.Database;

        return builder.AddContainer("postgres", options.Image)
            .WithEndpoint(name: "postgres", port: options.Port, targetPort: options.TargetPort)
            .WithEnvironment("POSTGRES_USER", postgresUser)
            .WithEnvironment("POSTGRES_PASSWORD", postgresPassword)
            .WithEnvironment("POSTGRES_DB", postgresDb);
    }

    public static string BuildConnectionString(
        PostgresOptions options,
        PostgresCredentialsOptions credentials)
    {
        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? credentials.User;
        var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? credentials.Password;
        var postgresDb = options.Database;

        return $"Host=postgres;Port=5432;Username={postgresUser};Password={postgresPassword};Database={postgresDb}";
    }
}

