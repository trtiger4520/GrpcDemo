namespace Grpc.Server;

public interface IDatetimeProvider
{
    DateTimeOffset Now { get; }
}

public class DatetimeProvider : IDatetimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.Now;

}