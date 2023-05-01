using Grpc.Servce;

namespace Grpc.ServerTests;

internal class FakeDatetimeProvider : IDatetimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
