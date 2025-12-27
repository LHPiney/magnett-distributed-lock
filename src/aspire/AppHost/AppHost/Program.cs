using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddContainer("postgres", "postgres:16")
    .WithEndpoint(name: "postgres", port: 5433, targetPort: 5432)
    .WithEnvironment("POSTGRES_USER", "dev")
    .WithEnvironment("POSTGRES_PASSWORD", "devpass")
    .WithEnvironment("POSTGRES_DB", "locksdb");

var rabbitmq = builder.AddContainer("rabbitmq", "rabbitmq:3.13-management")
    .WithEndpoint(name: "rabbitmq-amqp", port: 5672, targetPort: 5672)
    .WithEndpoint(name: "rabbitmq-ui", port: 15672, targetPort: 15672)
    .WithEnvironment("RABBITMQ_DEFAULT_USER", "dev")
    .WithEnvironment("RABBITMQ_DEFAULT_PASS", "devpass");

var memcached = builder.AddContainer("memcached", "memcached:1.6")
    .WithEndpoint(name: "memcached", port: 11211, targetPort: 11211);

var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak:26.0.2")
    .WithEndpoint(name: "keycloak-http", port: 8090, targetPort: 8080)
    .WithBindMount("../infra/keycloak/realm-export.json", "/opt/keycloak/data/import/realm-export.json", isReadOnly: true)
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin123")
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithEnvironment("KC_METRICS_ENABLED", "true")
    .WithArgs("start-dev", "--import-realm", "--http-port", "8080");

var api = builder.AddProject<Locks_Api>("locks-api")
    .WithEnvironment("POSTGRES_CONNECTION", "Host=postgres;Port=5432;Username=dev;Password=devpass;Database=locksdb")
    .WithEnvironment("RABBITMQ_HOST", "rabbitmq")
    .WithEnvironment("RABBITMQ_USER", "dev")
    .WithEnvironment("RABBITMQ_PASSWORD", "devpass")
    .WithEnvironment("MEMCACHED_ENDPOINT", "memcached:11211")
    .WithEnvironment("AUTH_AUTHORITY", "http://keycloak:8080/realms/locks")
    .WithEnvironment("AUTH_AUDIENCE", "locks-api");

var worker = builder.AddProject<Audit_Worker>("audit-worker")
    .WithEnvironment("POSTGRES_CONNECTION", "Host=postgres;Port=5432;Username=dev;Password=devpass;Database=locksdb")
    .WithEnvironment("RABBITMQ_HOST", "rabbitmq")
    .WithEnvironment("RABBITMQ_USER", "dev")
    .WithEnvironment("RABBITMQ_PASSWORD", "devpass");

var managementApp = builder.AddContainer("management-app", "management-app:latest")
    .WithEndpoint(name: "management-app", port: 3000, targetPort: 80)
    .WithEnvironment("NG_APP_OIDC_ISSUER", "http://keycloak:8080/realms/locks")
    .WithEnvironment("NG_APP_OIDC_CLIENT_ID", "portal")
    .WithEnvironment("NG_APP_API_BASE_URL", "http://locks-api:8080");

builder.Build().Run();
