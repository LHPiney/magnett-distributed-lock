using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Magnett.Locks.Infrastructure.PostgresDb.Data;

public sealed class LockDbContextFactory : IDesignTimeDbContextFactory<LockDbContext>
{
    public LockDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LockDbContext>();
        
        var connectionString = GetConnectionString();
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new LockDbContext(optionsBuilder.Options);
    }

    private static string GetConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "aspire", "AppHost", "AppHost");
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory();
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var servicesConfig = configuration.GetSection("Services");
        var credentialsConfig = configuration.GetSection("Credentials");

        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var database = servicesConfig["Postgres:Database"] ?? "locksdb";
        var user = credentialsConfig["Postgres:User"] 
            ?? Environment.GetEnvironmentVariable("POSTGRES_USER") 
            ?? "dev";
        var password = credentialsConfig["Postgres:Password"] 
            ?? Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") 
            ?? "devpass";

        return $"Host={host};Port={port};Database={database};Username={user};Password={password}";
    }
}

