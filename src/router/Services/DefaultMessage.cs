namespace MessageRouter.Services;

public class DefaultMessage
{
    public Guid ObjectId { get; init; }
    public string Sequence { get; init;}
    public DateTime Timestamp { get; init;}
    public string Payload { get; init; }
    
    public string? PartitionId { get; set; }
}