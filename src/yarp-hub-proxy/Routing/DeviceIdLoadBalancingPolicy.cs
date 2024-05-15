using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

namespace YarpHubProxy.Routing;

[Obsolete("This class is not used in the current implementation.")]
public class DeviceIdLoadBalancingPolicy : ILoadBalancingPolicy
{
    public const string PolicyName = "DeviceId";
    
    public DestinationState? PickDestination(HttpContext context, ClusterState cluster, IReadOnlyList<DestinationState> availableDestinations)
    {
        //var hub = context.Items["Forward-To-Hub"]?.ToString();
        var hub = context.Request.Headers["X-Forwarded-To-Hub"].FirstOrDefault();

        return string.IsNullOrEmpty(hub) ? null : availableDestinations.FirstOrDefault(state => state.DestinationId == hub);
    }

    public string Name { get; } = PolicyName;
}