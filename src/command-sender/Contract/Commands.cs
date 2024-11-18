using System.Text.Json.Serialization;

namespace CommandSender.Contract;

public abstract class Command
{
    [JsonIgnore]
    public abstract string Name { get; }
}

public class IncreaseCoolingCommand(string DeviceId): Command
{
    public override string Name => "IncreaseCooling";
}