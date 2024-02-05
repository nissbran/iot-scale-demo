using Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var daprResourcePath = Path.Combine(builder.AppHostDirectory, "../../dapr/components/");
var daprComponentOptions = new DaprComponentOptions() { LocalPath = daprResourcePath };

var commandsComponent = builder.AddDaprComponent("commands", "pubsub", daprComponentOptions); 
var deviceCatalogComponent = builder.AddDaprComponent("device-catalog", "state", daprComponentOptions);

builder.AddProject<Projects.MessageRouter>("message-router")
    .WithDaprSidecar()
    .WithReference(commandsComponent)
    .WithReference(deviceCatalogComponent);

builder.AddProject<Projects.CommandSender>("command-sender")
    .WithDaprSidecar()
    .WithReference(commandsComponent)
    .WithReference(deviceCatalogComponent);

//builder.AddProject<Projects.TemperatureDevice>("device-sim-1");

builder.Build().Run();