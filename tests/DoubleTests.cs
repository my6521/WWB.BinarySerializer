using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class DoubleTests
{
    [Theory]
    [InlineData(-123.5d)]
    [InlineData(double.MaxValue)]
    public void RoundTrip(double value) => Assert.Equal(value, BinarySerializer.DeserializeObject<DoubleContract>(BinarySerializer.SerializeObject(new DoubleContract { Value = value })).Value);
}

[BinaryContract]
public class DoubleContract { [BinaryField(1)] public double Value { get; set; } }
