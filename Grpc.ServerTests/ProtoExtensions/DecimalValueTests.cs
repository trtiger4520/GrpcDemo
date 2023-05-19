using Grpc.Server.Services;

namespace Grpc.ServerTests.ProtoExtensions;

[TestFixture]
public class DecimalValueTests
{
    private decimal NanoFactor = 1_000_000_000;

    [Test]
    public void TestDecimalConvertDecimalValueUnitsDoesWork()
    {
        var value = 100.01m;

        DecimalValue decimalValue = value;

        Assert.That(decimalValue.Units, Is.EqualTo(100));
    }
    [Test]
    public void TestDecimalConvertDecimalValueNanosDoesWork()
    {
        var value = 100.01m;

        DecimalValue decimalValue = value;

        Assert.That(decimalValue.Nanos, Is.EqualTo(0.01m * NanoFactor));
    }
    [Test]
    public void TestDecimalConvertDecimalValueWork()
    {
        var value = 180.0101m;

        DecimalValue decimalValue = value;
        var returnValue = (decimal)decimalValue;

        Assert.That(returnValue, Is.EqualTo(value));
    }
}