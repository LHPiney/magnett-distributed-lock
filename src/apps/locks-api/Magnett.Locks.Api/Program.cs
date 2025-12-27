using Enyim.Caching.Configuration;
using Magnett.Locks.Api.Options;
using Magnett.Locks.Api.Services;
using Magnett.Locks.Domain.Repositories;
using Magnett.Locks.Domain.Services;
using Magnett.Locks.Infrastructure.Memcached.Options;
using Magnett.Locks.Infrastructure.Memcached.Services;
using Magnett.Locks.Infrastructure.PostgresDb.Data;
using Magnett.Locks.Infrastructure.PostgresDb.Repositories;
using Magnett.Locks.Infrastructure.RabbitMQ.Options;
using Magnett.Locks.Infrastructure.RabbitMQ.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddGrpc();

builder.Services.Configure<PostgresOptions>(options =>
{
    options.ConnectionString = builder.Configuration["POSTGRES_CONNECTION"] 
        ?? throw new InvalidOperationException("POSTGRES_CONNECTION is required");
});

builder.Services.Configure<Magnett.Locks.Infrastructure.Memcached.Options.MemcachedOptions>(options =>
{
    options.Endpoint = builder.Configuration["MEMCACHED_ENDPOINT"] 
        ?? throw new InvalidOperationException("MEMCACHED_ENDPOINT is required");
});

builder.Services.Configure<Magnett.Locks.Infrastructure.RabbitMQ.Options.RabbitMqOptions>(options =>
{
    options.Host = builder.Configuration["RABBITMQ_HOST"] 
        ?? throw new InvalidOperationException("RABBITMQ_HOST is required");
    options.User = builder.Configuration["RABBITMQ_USER"] 
        ?? throw new InvalidOperationException("RABBITMQ_USER is required");
    options.Password = builder.Configuration["RABBITMQ_PASSWORD"] 
        ?? throw new InvalidOperationException("RABBITMQ_PASSWORD is required");
});

builder.Services.Configure<KestrelOptions>(builder.Configuration.GetSection(KestrelOptions.SectionName));

builder.Services.AddDbContext<LockDbContext>((sp, options) =>
{
    var postgresOptions = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
    options.UseNpgsql(postgresOptions.ConnectionString);
});

builder.Services.AddScoped<ILockRepository, LockRepository>();

var memcachedEndpoint = builder.Configuration["MEMCACHED_ENDPOINT"] 
    ?? throw new InvalidOperationException("MEMCACHED_ENDPOINT is required");

builder.Services.AddEnyimMemcached(options =>
{
    var parts = memcachedEndpoint.Split(':');
    options.AddServer(parts[0], int.Parse(parts[1]));
});

builder.Services.AddSingleton<ICacheService, Magnett.Locks.Infrastructure.Memcached.Services.CacheService>();

builder.Services.AddSingleton<IAuditService, Magnett.Locks.Infrastructure.RabbitMQ.Services.AuditService>();

builder.Services.AddSingleton<IOwnerIdProvider, OwnerIdProvider>();

builder.Services.AddScoped<ILockService, Magnett.Locks.Domain.Services.Implementations.LockService>();

builder.WebHost.ConfigureKestrel((context, options) =>
{
    var kestrelOptions = context.Configuration.GetSection(KestrelOptions.SectionName).Get<KestrelOptions>() 
        ?? new KestrelOptions();
    options.ListenAnyIP(kestrelOptions.HttpPort, listen => listen.Protocols = HttpProtocols.Http1AndHttp2);
    options.ListenAnyIP(kestrelOptions.GrpcPort, listen => listen.Protocols = HttpProtocols.Http2);
});

var app = builder.Build();

var postgresOptionsAtRuntime = app.Services.GetRequiredService<IOptions<PostgresOptions>>().Value;
await WaitForDatabaseAsync(postgresOptionsAtRuntime.ConnectionString, app.Logger);

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LockDbContext>();
    await dbContext.Database.MigrateAsync();
}

static async Task WaitForDatabaseAsync(string connectionString, ILogger logger)
{
    const int maxRetries = 30;
    const int delaySeconds = 2;
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            await connection.CloseAsync();
            logger.LogInformation("Database connection successful");
            return;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database not ready, retrying in {Delay}s (attempt {Attempt}/{MaxRetries})", 
                delaySeconds, i + 1, maxRetries);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
    }
    
    throw new InvalidOperationException($"Database not available after {maxRetries} attempts");
}

app.MapDefaultEndpoints();
app.MapHealthChecks("/v1/health");
app.MapHealthChecks("/v1/ready", new HealthCheckOptions { Predicate = _ => true });
app.MapGrpcService<LockServiceEndpoint>();
app.MapGet("/v1/ping", () => Results.Ok(new { status = "ok" }));

app.Run();
