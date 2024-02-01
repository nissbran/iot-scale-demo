namespace MessageRouter.Services;

public static class KafkaDefaultConf
{
    public const string DefaultGroupId = "testdata-consumer-group";
    public const string DefaultTopic = "testdata";
    public const string PinnedDefaultTopic = "pinned-testdata";
}