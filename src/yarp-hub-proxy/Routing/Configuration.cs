using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.LoadBalancing;

namespace YarpHubProxy.Routing;

public static class Configuration
{
    public static void AddIotHubProxy(this IServiceCollection services)
    {
        services.AddReverseProxy()
            .ConfigurePathRouting()
            .UseIotHubRequestTransform();
    }
    
    public static IReverseProxyBuilder ConfigureHeaderRouting(this IReverseProxyBuilder builder)
    {
        builder.Services.AddSingleton<IProxyConfigProvider, IotHubHeaderProxyConfigProvider>();
        return builder;
    }
    
    public static IReverseProxyBuilder ConfigurePathRouting(this IReverseProxyBuilder builder)
    {
        builder.Services.AddSingleton<IProxyConfigProvider, IotHubPathProxyConfigProvider>();
        return builder;
    }
    
    public static IReverseProxyBuilder UseDeviceIdLoadBalancing(this IReverseProxyBuilder builder)
    {
        builder.Services.AddSingleton<ILoadBalancingPolicy, DeviceIdLoadBalancingPolicy>();
        return builder;
    }
    
    public static IReverseProxyBuilder UseIotHubRequestTransform(this IReverseProxyBuilder builder)
    {
        builder.AddTransforms<IotHubSasAuthorizationRequestTransformProvider>();
        return builder;
    }
}