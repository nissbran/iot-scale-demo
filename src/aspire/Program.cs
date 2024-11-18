using Aspire.Hosting.Dapr;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", true);

var daprResourcePath = Path.Combine(builder.AppHostDirectory, "../../dapr/components/");
var daprComponentOptions = new DaprComponentOptions() { LocalPath = daprResourcePath };

var commandsComponent = builder.AddDaprComponent("commands", "pubsub", daprComponentOptions); 
var deviceCatalogComponent = builder.AddDaprComponent("device-catalog", "state", daprComponentOptions);

builder.AddProject<Projects.MessageMediator>("message-mediator")
    .WithDaprSidecar()
    .WithReference(commandsComponent)
    .WithReference(deviceCatalogComponent);

builder.AddProject<Projects.CommandSender>("command-sender")
    .WithDaprSidecar()
    .WithReference(commandsComponent)
    .WithReference(deviceCatalogComponent);

builder.AddProject<Projects.TelemetryIngestionApi>("telemetry-ingestion-api");

builder.AddProject<Projects.TemperatureDevice>("temperature-device");

builder.Build().Run();