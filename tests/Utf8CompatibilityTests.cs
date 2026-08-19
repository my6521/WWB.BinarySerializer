using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class Utf8CompatibilityTests
{
    [Fact]
    public void RoundTrip_AsciiSubsetUsesUtf8ByteLengthPrefix()
    {
        var bytes = BinarySerializer.SerializeObject(new AsciiContract { Value = "ABC" });

        Assert.Equal(new byte[] { 3, (byte)'A', (byte)'B', (byte)'C' }, bytes);
        Assert.Equal("ABC", BinarySerializer.DeserializeObject<AsciiContract>(bytes).Value);
    }
}

[BinaryContract]
public class AsciiContract
{
    [BinaryField(1)]
    public string Value { get; set; } = string.Empty;
}
