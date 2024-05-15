using Microsoft.Azure.Devices;
using Yarp.ReverseProxy.Configuration;

namespace YarpHubProxy.Routing;

[Obsolete("This class is not used in the current implementation.")]
public class IotHubLoadBalancingProxyConfigProvider(IConfiguration configuration) : IProxyConfigProvider
{
    private readonly Dictionary<string, IotHubConnectionStringBuilder> _iotHubs = new()
    {
        { IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubA"]).HostName, IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubA"]) },
        { IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubB"]).HostName, IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubA"]) }
    };

    public IProxyConfig GetConfig()
    {
        var destinations = new Dictionary<string, DestinationConfig>();

        foreach (var iotHub in _iotHubs)
        {
            destinations.Add(iotHub.Key, new DestinationConfig()
            {
                Address = $"https://{iotHub.Value.HostName}"
            });
        }
        
        var routers = new[]
        {
            new RouteConfig()
            {
                RouteId = "iothub-service",
                ClusterId = "iothubs",
                Match = new RouteMatch()
                {
                    Path = "/iothub-service/{*remainder}"
                }
            }
        };
        
        var clusters = new[]
        {
            new ClusterConfig()
            {
                ClusterId = "iothubs",
                LoadBalancingPolicy = DeviceIdLoadBalancingPolicy.PolicyName,
                Destinations = destinations
            }
        };

        return new IotHubProxyConfig(routers, clusters);
    }
}