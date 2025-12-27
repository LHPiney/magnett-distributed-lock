using Aspire.Hosting;
using Magnett.Locks.AppHost.Options;

namespace Magnett.Locks.AppHost.Extensions;

public static class RabbitMqExtensions
{
    public static IResourceBuilder<ContainerResource> AddRabbitMqContainer(
        this IDistributedApplicationBuilder builder,
        RabbitMqOptions options,
        RabbitMqCredentialsOptions credentials)
    {
        var rabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? credentials.User;
        var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? credentials.Password;

        return builder.AddContainer("rabbitmq", options.Image)
            .WithEndpoint(name: "rabbitmq-amqp", port: options.AmqpPort, targetPort: 5672)
            .WithEndpoint(name: "rabbitmq-ui", port: options.UiPort, targetPort: 15672)
            .WithEnvironment("RABBITMQ_DEFAULT_USER", rabbitMqUser)
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", rabbitMqPassword);
    }

    public static (string User, string Password) GetCredentials(RabbitMqCredentialsOptions credentials)
    {
        return (
            Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? credentials.User,
            Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? credentials.Password
        );
    }
}

