namespace Grpc.Client;

public class Command
{
    public string Value { get; init; }

    public Command(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Command command) => command.Value;
    public static implicit operator Command(string value) => new(value);
}