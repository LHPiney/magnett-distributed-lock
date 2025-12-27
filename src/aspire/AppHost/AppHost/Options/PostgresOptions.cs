namespace Magnett.Locks.AppHost.Options;

public sealed class PostgresOptions
{
    public const string SectionName = "Services:Postgres";
    
    public string Image { get; set; } = "postgres:16";
    public int Port { get; set; } = 5433;
    public int TargetPort { get; set; } = 5432;
    public string Database { get; set; } = "locksdb";
}

public sealed class PostgresCredentialsOptions
{
    public const string SectionName = "Credentials:Postgres";
    
    public string User { get; set; } = "dev";
    public string Password { get; set; } = "devpass";
}

