using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class Int64Tests
{
    [Theory]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    public void RoundTrip(long value) => Assert.Equal(value, BinarySerializer.DeserializeObject<Int64Contract>(BinarySerializer.SerializeObject(new Int64Contract { Value = value })).Value);
}

[BinaryContract]
public class Int64Contract { [BinaryField(1)] public long Value { get; set; } }
