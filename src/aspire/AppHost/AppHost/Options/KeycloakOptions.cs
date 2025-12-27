namespace Magnett.Locks.AppHost.Options;

public sealed class KeycloakOptions
{
    public const string SectionName = "Services:Keycloak";
    
    public string Image { get; set; } = "quay.io/keycloak/keycloak:26.0.2";
    public int Port { get; set; } = 8090;
    public int TargetPort { get; set; } = 8080;
}

public sealed class KeycloakCredentialsOptions
{
    public const string SectionName = "Credentials:Keycloak";
    
    public string Admin { get; set; } = "admin";
    public string AdminPassword { get; set; } = "admin123";
}

public sealed class KeycloakConfigOptions
{
    public const string SectionName = "Keycloak";
    
    public string Realm { get; set; } = "locks";
    public string ClientId { get; set; } = "portal";
}

