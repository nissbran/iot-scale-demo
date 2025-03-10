﻿using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.OpenTelemetry;
using Serilog.Sinks.SystemConsole.Themes;

namespace MessageMediator.Telemetry;

internal static class ObservabilityConfiguration
{
    internal static bool UsePrometheusEndpoint { get; private set; }

    public static WebApplicationBuilder ConfigureTelemetry(this WebApplicationBuilder builder)
    {
        UsePrometheusEndpoint = builder.Configuration.GetValue<bool>("USE_PROMETHEUS_ENDPOINT");

        var resourceBuilder = ResourceBuilder.CreateDefault();

        var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            builder.Services.AddLogging(logging =>
            {
                logging.AddOpenTelemetry(builderOptions =>
                {
                    builderOptions.SetResourceBuilder(resourceBuilder);
                    builderOptions.IncludeFormattedMessage = true;
                    builderOptions.IncludeScopes = false;
                    builderOptions.AddAzureMonitorLogExporter(options => options.ConnectionString = appInsightsConnectionString);
                });
            });
        }
        else
        {
            builder.Host.UseSerilog((context, configuration) =>
            {
                var serilogConfiguration = configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Filter.With<IgnoredEndpointsLogFilter>()
                    .Enrich.FromLogContext();

                if (context.HostingEnvironment.IsDevelopment() || builder.Configuration.GetValue<bool>("USE_CONSOLE_LOG_OUTPUT"))
                {
                    if (builder.Configuration.GetValue<bool>("USE_CONSOLE_JSON_LOG_OUTPUT"))
                    {
                        serilogConfiguration.WriteTo.Console(formatter: new RenderedCompactJsonFormatter());
                    }
                    else
                    {
                        serilogConfiguration.WriteTo.Console(theme: AnsiConsoleTheme.Sixteen);
                    }
                }

                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    var protocol = context.Configuration["OTEL_EXPORTER_OTLP_PROTOCOL"] == "http/protobuf"
                        ? OtlpProtocol.HttpProtobuf
                        : OtlpProtocol.Grpc;
                    Log.Verbose("Using OpenTelemetry Protocol (OTLP) endpoint {OtlpEndpoint} with {Proctocol}, for Serilog", otlpEndpoint, protocol);
                    serilogConfiguration.WriteTo.OpenTelemetry(options =>
                    {
                        options.HttpMessageHandler = new SocketsHttpHandler { ActivityHeadersPropagator = null };
                        options.Protocol = protocol;
                        options.Endpoint = otlpEndpoint;
                        options.Headers = GetSerilogSpecificOtelHeaders(builder);
                        options.ResourceAttributes = resourceBuilder.Build().Attributes.ToDictionary();
                    });
                }

                var seqEndpoint = context.Configuration["SEQ_ENDPOINT"];
                if (!string.IsNullOrEmpty(seqEndpoint))
                {
                    serilogConfiguration.WriteTo.Seq(seqEndpoint);
                }
            });
        }

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracingBuilder =>
            {
                if (!string.IsNullOrEmpty(appInsightsConnectionString))
                {
                    tracingBuilder.AddAzureMonitorTraceExporter(options =>
                    {
                        options.ConnectionString = appInsightsConnectionString;
                        //options.SamplingRatio = 0.1f;
                    });
                }

                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracingBuilder.AddOtlpExporter();
                }

                tracingBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddAspNetCoreInstrumentation(options => options.Filter = TraceEndpointsFilter);
            })
            .WithMetrics(metricsBuilder =>
            {
                if (UsePrometheusEndpoint)
                {
                    metricsBuilder.AddPrometheusExporter();
                }

                if (!string.IsNullOrEmpty(appInsightsConnectionString))
                {
                    metricsBuilder.AddAzureMonitorMetricExporter(options => { options.ConnectionString = appInsightsConnectionString; });
                }

                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    metricsBuilder.AddOtlpExporter();
                }

                metricsBuilder
                    .AddConsumedEventsMetrics()
                    .AddIotHubConnectionMetrics()
                    .SetResourceBuilder(resourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation();
            });

        return builder;
    }

    private static bool TraceEndpointsFilter(HttpContext httpContext)
    {
        try
        {
            return httpContext.Request.Path.Value != "/healthz" && httpContext.Request.Path.Value != "/metrics";
        }
        catch
        {
            return true;
        }
    }
    
    private static Dictionary<string, string> GetSerilogSpecificOtelHeaders(WebApplicationBuilder builder)
    {
        var headerDictionary = new Dictionary<string, string>();
        try
        {
            var headers = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"];
            var apiKey = headers?.Split(";").FirstOrDefault(h => h.StartsWith("x-otlp-api-key"))?.Split("=")[1];
            if (!string.IsNullOrEmpty(apiKey))
            {
                headerDictionary.Add("x-otlp-api-key", apiKey);
            }
        }
        catch (Exception e)
        {
            Log.Verbose(e, "Error while reading OTEL_EXPORTER_OTLP_HEADERS: {Error}", e.Message);
        }
        return headerDictionary;
    }
}