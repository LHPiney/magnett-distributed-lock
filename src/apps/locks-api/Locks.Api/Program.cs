using Locks.Api.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddGrpc();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, listen => listen.Protocols = HttpProtocols.Http1AndHttp2);
    options.ListenAnyIP(8081, listen => listen.Protocols = HttpProtocols.Http2);
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapHealthChecks("/v1/health");
app.MapHealthChecks("/v1/ready", new HealthCheckOptions { Predicate = _ => true });
app.MapGrpcService<LockServiceEndpoint>();
app.MapGet("/v1/ping", () => Results.Ok(new { status = "ok" }));

app.Run();
