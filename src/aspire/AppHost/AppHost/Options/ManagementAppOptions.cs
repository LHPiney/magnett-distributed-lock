namespace Magnett.Locks.AppHost.Options;

public sealed class ManagementAppOptions
{
    public const string SectionName = "Services:ManagementApp";
    
    public string Image { get; set; } = "management-app:latest";
    public int Port { get; set; } = 3000;
    public int TargetPort { get; set; } = 80;
}

