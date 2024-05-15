using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Security;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace YarpHubProxy.Routing;

public class IotHubSasAuthorizationRequestTransformProvider : ITransformProvider
{
    private readonly ILogger<IotHubSasAuthorizationRequestTransformProvider> _logger;
    private readonly Dictionary<string, IotHubConnectionStringBuilder> _iotHubs;
    
    public IotHubSasAuthorizationRequestTransformProvider(IConfiguration configuration, ILogger<IotHubSasAuthorizationRequestTransformProvider> logger)
    {
        _logger = logger;
        _iotHubs = new Dictionary<string, IotHubConnectionStringBuilder>
        {
            { IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubA"]).HostName, IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubA"]) },
            { IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubB"]).HostName, IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubB"]) }
        };
    }
    
    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(transformContext =>
        {
            _logger.LogDebug("Fetching hub credentials for {DestinationPrefix}", transformContext.DestinationPrefix);
            
            var hub = transformContext.DestinationPrefix.Replace("https://", "");
            var builder = _iotHubs.FirstOrDefault(x => x.Key == hub).Value;

            // TODO: Add error handling for missing hub
            // TODO: Add sas token caching
            var sasBuilder = new SharedAccessSignatureBuilder
            {
                Key = builder.SharedAccessKey,
                KeyName = builder.SharedAccessKeyName,
                Target = builder.HostName,
                TimeToLive = TimeSpan.FromHours(1)
            };
            var sasToken = sasBuilder.ToSignature();
            transformContext.ProxyRequest.Headers.Add("Authorization", sasToken);
            
            return default;
        });
    }
}