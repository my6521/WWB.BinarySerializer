using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class SByteTests
{
    [Theory]
    [InlineData((sbyte)-128)]
    [InlineData((sbyte)127)]
    public void RoundTrip(sbyte value) => Assert.Equal(value, BinarySerializer.DeserializeObject<SByteContract>(BinarySerializer.SerializeObject(new SByteContract { Value = value })).Value);
}

[BinaryContract]
public class SByteContract { [BinaryField(1)] public sbyte Value { get; set; } }
