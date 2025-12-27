namespace Magnett.Locks.AppHost.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "Services:RabbitMQ";
    
    public string Image { get; set; } = "rabbitmq:3.13-management";
    public int AmqpPort { get; set; } = 5672;
    public int UiPort { get; set; } = 15672;
}

public sealed class RabbitMqCredentialsOptions
{
    public const string SectionName = "Credentials:RabbitMQ";
    
    public string User { get; set; } = "dev";
    public string Password { get; set; } = "devpass";
}

