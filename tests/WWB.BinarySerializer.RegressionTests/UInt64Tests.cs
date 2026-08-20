using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class UInt64Tests
{
    [Theory]
    [InlineData((ulong)0)]
    [InlineData(ulong.MaxValue)]
    public void RoundTrip(ulong value) => Assert.Equal(value, BinarySerializer.DeserializeObject<UInt64Contract>(BinarySerializer.SerializeObject(new UInt64Contract { Value = value })).Value);
}

[BinaryContract]
public class UInt64Contract { [BinaryField(1)] public ulong Value { get; set; } }
