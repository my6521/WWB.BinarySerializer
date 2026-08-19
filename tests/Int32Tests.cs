using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class Int32Tests
{
    [Fact]
    public void Contract_HasCompileTimeRegisteredCodec()
    {
        Assert.NotNull(GeneratedCodecRegistry<Int32Contract>.Instance);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void RoundTrip(int value) => Assert.Equal(value, BinarySerializer.DeserializeObject<Int32Contract>(BinarySerializer.SerializeObject(new Int32Contract { Value = value })).Value);
}

[BinaryContract]
public class Int32Contract { [BinaryField(1)] public int Value { get; set; } }
