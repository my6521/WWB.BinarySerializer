using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class ByteArrayTests
{
    [Fact]
    public void RoundTrip_VariableLengthPayload()
    {
        var value = Enumerable.Range(0, 256).Select(index => (byte)index).ToArray();
        Assert.Equal(value, BinarySerializer.DeserializeObject<ByteArrayContract>(BinarySerializer.SerializeObject(new ByteArrayContract { Value = value })).Value);
    }
}

[BinaryContract]
public class ByteArrayContract { [BinaryField(1, LengthPrefixSize = 2)] public byte[] Value { get; set; } = Array.Empty<byte>(); }
