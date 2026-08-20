using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class BooleanTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public void RoundTrip(bool value, byte expected)
    {
        var bytes = BinarySerializer.SerializeObject(new BooleanContract { Value = value });
        Assert.Equal(new[] { expected }, bytes);
        Assert.Equal(value, BinarySerializer.DeserializeObject<BooleanContract>(bytes).Value);
    }
}

[BinaryContract]
public class BooleanContract { [BinaryField(1)] public bool Value { get; set; } }
