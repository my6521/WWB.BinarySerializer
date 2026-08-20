using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class UInt32Tests
{
    [Theory]
    [InlineData((uint)0)]
    [InlineData(uint.MaxValue)]
    public void RoundTrip(uint value) => Assert.Equal(value, BinarySerializer.DeserializeObject<UInt32Contract>(BinarySerializer.SerializeObject(new UInt32Contract { Value = value })).Value);
}

[BinaryContract]
public class UInt32Contract { [BinaryField(1)] public uint Value { get; set; } }
