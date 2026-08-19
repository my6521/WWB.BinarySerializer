using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class FloatTests
{
    [Theory]
    [InlineData(-123.5f)]
    [InlineData(float.MaxValue)]
    public void RoundTrip(float value) => Assert.Equal(value, BinarySerializer.DeserializeObject<FloatContract>(BinarySerializer.SerializeObject(new FloatContract { Value = value })).Value);
}

[BinaryContract]
public class FloatContract { [BinaryField(1)] public float Value { get; set; } }
