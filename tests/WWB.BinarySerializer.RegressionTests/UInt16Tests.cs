using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class UInt16Tests
{
    [Theory]
    [InlineData((ushort)0)]
    [InlineData(ushort.MaxValue)]
    public void RoundTrip(ushort value) => Assert.Equal(value, BinarySerializer.DeserializeObject<UInt16Contract>(BinarySerializer.SerializeObject(new UInt16Contract { Value = value })).Value);
}

[BinaryContract]
public class UInt16Contract { [BinaryField(1)] public ushort Value { get; set; } }
