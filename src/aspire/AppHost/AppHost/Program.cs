using Aspire.Hosting;
using Magnett.Locks.AppHost.Extensions;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath ?? Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var config = AppHostConfigurationExtensions.LoadConfiguration(configuration);

var postgres = builder.AddPostgresContainer(config.PostgresOptions, config.PostgresCredentials);
var rabbitmq = builder.AddRabbitMqContainer(config.RabbitMqOptions, config.RabbitMqCredentials);
var memcached = builder.AddMemcachedContainer(config.MemcachedOptions);
var keycloak = builder.AddKeycloakContainer(config.KeycloakOptions, config.KeycloakCredentials);

var postgresConnection = PostgresExtensions.BuildConnectionString(config.PostgresOptions, config.PostgresCredentials);
var rabbitMqCredentials = RabbitMqExtensions.GetCredentials(config.RabbitMqCredentials);

var api = builder.AddLocksApiProject(postgresConnection, rabbitMqCredentials, config.KeycloakConfig);
var worker = builder.AddAuditWorkerProject(postgresConnection, rabbitMqCredentials);
var managementApp = builder.AddManagementAppContainer(config.ManagementAppOptions, config.KeycloakConfig);

builder.Build().Run();
