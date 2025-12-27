using Aspire.Hosting;
using Magnett.Locks.AppHost.Options;
using Projects;

namespace Magnett.Locks.AppHost.Extensions;

public static class AuditWorkerExtensions
{
    public static IResourceBuilder<ProjectResource> AddAuditWorkerProject(
        this IDistributedApplicationBuilder builder,
        string postgresConnection,
        (string User, string Password) rabbitMqCredentials)
    {
        return builder.AddProject<Magnett_Locks_Audit_Worker>("audit-worker")
            .WithEnvironment("POSTGRES_CONNECTION", postgresConnection)
            .WithEnvironment("RABBITMQ_HOST", "rabbitmq")
            .WithEnvironment("RABBITMQ_USER", rabbitMqCredentials.User)
            .WithEnvironment("RABBITMQ_PASSWORD", rabbitMqCredentials.Password);
    }
}

