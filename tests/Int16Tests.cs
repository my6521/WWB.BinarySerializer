using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class Int16Tests
{
    [Theory]
    [InlineData(short.MinValue)]
    [InlineData(short.MaxValue)]
    public void RoundTrip(short value) => Assert.Equal(value, BinarySerializer.DeserializeObject<Int16Contract>(BinarySerializer.SerializeObject(new Int16Contract { Value = value })).Value);
}

[BinaryContract]
public class Int16Contract { [BinaryField(1)] public short Value { get; set; } }
