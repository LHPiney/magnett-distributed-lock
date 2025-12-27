using Magnett.Locks.AppHost.Options;
using Microsoft.Extensions.Configuration;

namespace Magnett.Locks.AppHost.Extensions;

public static class AppHostConfigurationExtensions
{
    public static AppHostConfiguration LoadConfiguration(IConfiguration configuration)
    {
        return new AppHostConfiguration
        {
            PostgresOptions = configuration.GetSection(PostgresOptions.SectionName).Get<PostgresOptions>() ?? new PostgresOptions(),
            PostgresCredentials = configuration.GetSection(PostgresCredentialsOptions.SectionName).Get<PostgresCredentialsOptions>() ?? new PostgresCredentialsOptions(),
            RabbitMqOptions = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions(),
            RabbitMqCredentials = configuration.GetSection(RabbitMqCredentialsOptions.SectionName).Get<RabbitMqCredentialsOptions>() ?? new RabbitMqCredentialsOptions(),
            KeycloakOptions = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>() ?? new KeycloakOptions(),
            KeycloakCredentials = configuration.GetSection(KeycloakCredentialsOptions.SectionName).Get<KeycloakCredentialsOptions>() ?? new KeycloakCredentialsOptions(),
            KeycloakConfig = configuration.GetSection(KeycloakConfigOptions.SectionName).Get<KeycloakConfigOptions>() ?? new KeycloakConfigOptions(),
            MemcachedOptions = configuration.GetSection(MemcachedOptions.SectionName).Get<MemcachedOptions>() ?? new MemcachedOptions(),
            ManagementAppOptions = configuration.GetSection(ManagementAppOptions.SectionName).Get<ManagementAppOptions>() ?? new ManagementAppOptions()
        };
    }
}

public sealed class AppHostConfiguration
{
    public PostgresOptions PostgresOptions { get; set; } = new();
    public PostgresCredentialsOptions PostgresCredentials { get; set; } = new();
    public RabbitMqOptions RabbitMqOptions { get; set; } = new();
    public RabbitMqCredentialsOptions RabbitMqCredentials { get; set; } = new();
    public KeycloakOptions KeycloakOptions { get; set; } = new();
    public KeycloakCredentialsOptions KeycloakCredentials { get; set; } = new();
    public KeycloakConfigOptions KeycloakConfig { get; set; } = new();
    public MemcachedOptions MemcachedOptions { get; set; } = new();
    public ManagementAppOptions ManagementAppOptions { get; set; } = new();
}

