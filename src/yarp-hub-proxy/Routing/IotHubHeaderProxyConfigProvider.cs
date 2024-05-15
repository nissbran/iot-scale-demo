using Microsoft.Azure.Devices;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace YarpHubProxy.Routing;

public class IotHubHeaderProxyConfigProvider(IConfiguration configuration) : IProxyConfigProvider
{
    private readonly Dictionary<string, IotHubConnectionStringBuilder> _iotHubs = new()
    {
        { IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubA"]).HostName, IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubA"]) },
        { IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubB"]).HostName, IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubB"]) }
    };
    private readonly List<KeyValuePair<string, string>> _iotGroups =
    [
        new(IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubA"]).HostName, "GroupA"),
        new(IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubB"]).HostName, "GroupB"),
        new(IotHubConnectionStringBuilder.Create(configuration["IotHubs:IotHubA"]).HostName, "GroupC")
    ];

    public IProxyConfig GetConfig()
    {
        var clusterConfigs = new List<ClusterConfig>();
        var routeConfigs = new List<RouteConfig>();
        
        foreach (var iotHub in _iotHubs)
        {
            clusterConfigs.Add(new ClusterConfig()
            {
                ClusterId = iotHub.Key,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { iotHub.Key, new DestinationConfig() { Address = $"https://{iotHub.Value.HostName}" } }
                }
            });
            
            var headerValues = _iotGroups.Where(x => x.Key == iotHub.Key).Select(x => x.Value).ToArray();
            
            routeConfigs.Add(new RouteConfig()
            {
                RouteId = iotHub.Key,
                ClusterId = iotHub.Key,
                Match = new RouteMatch()
                {
                    Path = "/iothub-service/{*remainder}",
                    Headers = new[]
                    {
                        new RouteHeader()
                        {
                            Name = "Client-Group",
                            Values = headerValues,
                            Mode = HeaderMatchMode.ExactHeader
                        }
                    }
                }
            }.WithTransformPathRemovePrefix("/iothub-service"));
        }
        
        return new IotHubProxyConfig(routeConfigs, clusterConfigs);
    }
}
