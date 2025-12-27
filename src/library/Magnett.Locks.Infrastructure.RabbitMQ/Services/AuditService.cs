using System.Text.Json;
using Magnett.Locks.Domain.Services;
using Magnett.Locks.Infrastructure.RabbitMQ.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Magnett.Locks.Infrastructure.RabbitMQ.Services;

public sealed class AuditService : IAuditService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<AuditService> _logger;
    private const string ExchangeName = "audit.events";
    private const string QueueName = "audit.locks";

    public AuditService(IOptions<RabbitMqOptions> options, ILogger<AuditService> logger)
    {
        _logger = logger;
        var rabbitMqOptions = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = rabbitMqOptions.Host,
            UserName = rabbitMqOptions.User,
            Password = rabbitMqOptions.Password
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(QueueName, ExchangeName, "lock.*");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize audit service");
            throw;
        }
    }

    public async Task RecordEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = new AuditEventDto
            {
                EventType = auditEvent.EventType,
                TenantId = auditEvent.TenantId,
                Environment = auditEvent.Environment,
                Namespace = auditEvent.Namespace,
                ResourceId = auditEvent.ResourceId,
                LockId = auditEvent.LockId,
                OwnerId = auditEvent.OwnerId,
                Timestamp = auditEvent.Timestamp,
                Outcome = auditEvent.Outcome,
                ErrorMessage = auditEvent.ErrorMessage
            };
            var json = JsonSerializer.Serialize(dto);
            var body = System.Text.Encoding.UTF8.GetBytes(json);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: auditEvent.EventType,
                basicProperties: properties,
                body: body
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record audit event {EventType}", auditEvent.EventType);
        }

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

    private sealed class AuditEventDto
    {
        public string EventType { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;
        public string? LockId { get; set; }
        public string? OwnerId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Outcome { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}

