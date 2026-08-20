using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class ByteTests
{
    [Theory]
    [InlineData((byte)0)]
    [InlineData(byte.MaxValue)]
    public void RoundTrip(byte value) => Assert.Equal(value, BinarySerializer.DeserializeObject<ByteContract>(BinarySerializer.SerializeObject(new ByteContract { Value = value })).Value);
}

[BinaryContract]
public class ByteContract { [BinaryField(1)] public byte Value { get; set; } }
