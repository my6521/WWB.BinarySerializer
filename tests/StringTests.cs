using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class StringTests
{
    [Fact]
    public void Deserialize_InvalidUtf8_IsRejectedWithPreciseOffset()
    {
        var exception = Assert.Throws<SerializationException>(() =>
            BinarySerializer.DeserializeObject<StringContract>(new byte[] { 2, 0xC3, 0x28 }));

        Assert.Equal(1, exception.Offset);
        Assert.Equal(typeof(StringContract), exception.ContractType);
        Assert.IsType<System.Text.DecoderFallbackException>(exception.InnerException);
    }
    [Theory]
    [InlineData("")]
    [InlineData("ABCD")]
    public void RoundTrip_DefaultUtf8Encoding(string value) => Assert.Equal(value, BinarySerializer.DeserializeObject<StringContract>(BinarySerializer.SerializeObject(new StringContract { Value = value })).Value);
}

[BinaryContract]
public class StringContract { [BinaryField(1)] public string Value { get; set; } = string.Empty; }
