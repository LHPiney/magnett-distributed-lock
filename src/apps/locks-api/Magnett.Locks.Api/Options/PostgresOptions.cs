namespace Magnett.Locks.Api.Options;

public sealed class PostgresOptions
{
    public const string SectionName = "Postgres";
    
    public string ConnectionString { get; set; } = string.Empty;
}

