namespace Grpc.Servce;

public interface IDatetimeProvider
{
    DateTimeOffset Now { get; }
}

public class DatetimeProvider : IDatetimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.Now;

}