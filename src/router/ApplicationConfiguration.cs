using System.Text;
using Dapr;
using MessageRouter.MessageHandlers;
using MessageRouter.Services;
using MessageRouter.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MessageRouter;

internal static class ApplicationConfiguration
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
        builder.Services.AddDaprClient();
        builder.Services.AddSingleton<EventConsumedMetrics>();
        builder.Services.AddSingleton<IotHubConnectionMetrics>();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();
        });

        builder.Services.AddSingleton<IKafkaConnectionProvider, EventHubKafkaConnectionProvider>();
        builder.Services.AddHostedService<ConsumerService>();

        builder.Services.AddSingleton<MessageMediator>();
        builder.Services.Scan(scan => scan
            .FromAssembliesOf(typeof(IMessageHandler))
            .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler)))
            .AsImplementedInterfaces()
        );
        
        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseCloudEvents();
        app.MapSubscribeHandler();
        app.UseHealthChecks("/healthz");
        if (ObservabilityConfiguration.UsePrometheusEndpoint)
        {
            app.MapPrometheusScrapingEndpoint().RequireHost("*:9090");
        }
        
        // app.Use(async (httpContext, next) =>
        // {
        //     try
        //     {
        //         httpContext.Request.EnableBuffering();
        //         string requestBody = await new StreamReader(httpContext.Request.Body, Encoding.UTF8).ReadToEndAsync();
        //         httpContext.Request.Body.Position = 0;
        //         Console.WriteLine($"Request body: {requestBody}");
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Exception reading request: {ex.Message}");
        //     }
        //
        //     Stream originalBody = httpContext.Response.Body;
        //     try
        //     {
        //         using var memStream = new MemoryStream();
        //         httpContext.Response.Body = memStream;
        //
        //         // call to the following middleware 
        //         // response should be produced by one of the following middlewares
        //         await next(httpContext); 
        //
        //         memStream.Position = 0;
        //         string responseBody = new StreamReader(memStream).ReadToEnd();
        //
        //         memStream.Position = 0;
        //         await memStream.CopyToAsync(originalBody);
        //         Console.WriteLine(responseBody);
        //     }
        //     finally
        //     {
        //         httpContext.Response.Body = originalBody;
        //     }
        // });

        app.MapPost("handler/messages", 
            
            [Topic("pubsub", "iot-events")] 
            [TopicMetadata("rawPayload", "true")]
            async (HttpContext context, [FromBody]DataMessage message) =>
        {
            Log.Information("Received message: {Temperature}.", message.Temperature);

            return TypedResults.Ok();
        });

        return app;
    }
}

public class DataMessage 
{
    public int Temperature { get; set; }

}