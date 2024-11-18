using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Mvc;
using TelemetryIngestionApi.Telemetry;

namespace TelemetryIngestionApi;

internal static class ApplicationConfiguration
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
        builder.Services.AddSingleton<TelemetryEventConsumedMetrics>();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();
        });
        
        // Certificate authentication custom validation service
        //builder.Services.AddSingleton<CertificateValidationService>();
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
            .AddCertificate(options =>
            {
                // Set the allowed certificate types to all (Chained and self-signed), default is Chained
                options.AllowedCertificateTypes = CertificateTypes.All;
                // These settings are to enable the use of chained self-signed certificates without having to install the root certificate
                // This means that we run without revocation checks and trusted root certificates
                // This is not recommended for production use
                // For production use, you should install the root certificate, use revocation checks and not allow self-signed certificates
                options.CustomTrustStore = GetChainCertificates(builder);
                options.ChainTrustValidationMode = X509ChainTrustMode.CustomRootTrust;
                options.RevocationMode = X509RevocationMode.NoCheck;
                
                options.Events = new CertificateAuthenticationEvents
                {
                    OnCertificateValidated = context =>
                    {
                        // If you want to add custom certificate validation logic, you can do so here
                        // var validationService = context.HttpContext.RequestServices.GetRequiredService<CertificateValidationService>();
                        //
                        // if (!validationService.ValidateCertificate(context.ClientCertificate))
                        // {
                        //     context.Fail("Invalid certificate");
                        //     return Task.CompletedTask;
                        // }
                        
                        var claims = new[]
                        {
                            new Claim(
                                ClaimTypes.NameIdentifier,
                                context.ClientCertificate.Subject,
                                ClaimValueTypes.String, context.Options.ClaimsIssuer),
                            new Claim(
                                ClaimTypes.Name,
                                context.ClientCertificate.Subject,
                                ClaimValueTypes.String, context.Options.ClaimsIssuer)
                        };
        
                        context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                        context.Success();
        
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.Fail("Certificate authentication failed");
                        return Task.CompletedTask;
                    }
                };
            })
            .AddCertificateCache(options =>
            {
                options.CacheSize = 2048;
                options.CacheEntryExpiration = TimeSpan.FromMinutes(30);
            });
        
        builder.Services.AddCertificateForwarding(options =>
        {
            options.CertificateHeader = "X-Client-Cert";

            options.HeaderConverter = headerValue =>
            {
                X509Certificate2? clientCertificate = null;

                if (!string.IsNullOrWhiteSpace(headerValue))
                {
                    clientCertificate = X509CertificateLoader.LoadCertificate(Convert.FromBase64String(headerValue));
                }
                return clientCertificate!;
            };
        });

        return builder.Build();
    }

    private static X509Certificate2Collection GetChainCertificates(WebApplicationBuilder builder)
    {
        var certificateCollection = new X509Certificate2Collection();
        certificateCollection.ImportFromPemFile(builder.Configuration["IntermediateCertFileHubA"]);
        certificateCollection.ImportFromPemFile(builder.Configuration["RootCertFileHubA"]);
        certificateCollection.ImportFromPemFile(builder.Configuration["IntermediateCertFileHubB"]);
        certificateCollection.ImportFromPemFile(builder.Configuration["RootCertFileHubB"]);
        return certificateCollection;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseHealthChecks("/healthz");
        if (ObservabilityConfiguration.UsePrometheusEndpoint)
        {
            app.MapPrometheusScrapingEndpoint().RequireHost("*:9090");
        }

        app.UseCertificateForwarding();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapPost("telemetry/temperature", ([FromBody] TemperatureTelemetry telemetry, 
            [FromServices] TelemetryEventConsumedMetrics metrics, 
            [FromServices] ILogger<Program> logger,
            HttpContext context) =>
        {
            var clientName = context.User.Identity?.Name;
            metrics.RecordTemperatureTelemetryValue(telemetry.Temperature);
            //logger.LogInformation("Received temperature telemetry value {Temperature} from {ClientName}", telemetry.Temperature, clientName);
            return Task.FromResult(TypedResults.Ok());
        }).RequireAuthorization(builder => builder.RequireAuthenticatedUser());
        
        return app;
    }
}

public class TemperatureTelemetry
{
    public long Temperature { get; set; }
}
