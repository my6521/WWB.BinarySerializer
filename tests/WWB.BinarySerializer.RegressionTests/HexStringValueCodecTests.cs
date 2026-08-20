using WWB.BinarySerializer.Attributes;
using WWB.BinarySerializer.Codecs.Text;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class HexStringValueCodecTests
{
    [Theory]
    [InlineData("", new byte[] { 0 })]
    [InlineData("00", new byte[] { 1, 0 })]
    [InlineData("abcd", new byte[] { 2, 0xAB, 0xCD })]
    public void RoundTrip_LengthPrefixedHex_UsesBinaryByteLength(string value, byte[] expected)
    {
        var runtime = new SerializerBuilder().AddHexCodecs().Build();

        var payload = runtime.Serialize(new HexStringContract { Value = value });
        var result = runtime.Deserialize<HexStringContract>(payload);

        Assert.Equal(expected, payload);
        Assert.Equal(value.ToUpperInvariant(), result.Value);
    }

    [Fact]
    public void RoundTrip_FixedLengthHex_HasNoPrefix()
    {
        var runtime = new SerializerBuilder().AddHexCodecs().Build();

        var payload = runtime.Serialize(new FixedHexStringContract { Value = "ABCD" });

        Assert.Equal(new byte[] { 0xAB, 0xCD }, payload);
        Assert.Equal("ABCD", runtime.Deserialize<FixedHexStringContract>(payload).Value);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("GG")]
    [InlineData("00 11")]
    public void Serialize_Hex_RejectsMalformedInput(string value)
    {
        var runtime = new SerializerBuilder().AddHexCodecs().Build();

        Assert.Throws<FormatException>(() => runtime.Serialize(new HexStringContract { Value = value }));
    }

    [Fact]
    public void Serialize_FixedLengthHex_RejectsWrongByteLength()
    {
        var runtime = new SerializerBuilder().AddHexCodecs().Build();

        Assert.Throws<ArgumentException>(() =>
            runtime.Serialize(new FixedHexStringContract { Value = "AA" }));
    }
}

[BinaryContract]
public sealed class HexStringContract
{
    [BinaryField(1, ValueCodecName = LengthPrefixedHexStringValueCodec.CodecName)]
    public string Value { get; set; } = string.Empty;
}

[BinaryContract]
public sealed class FixedHexStringContract
{
    [BinaryField(1, FixedLength = 2, ValueCodecName = FixedLengthHexStringValueCodec.CodecName)]
    public string Value { get; set; } = string.Empty;
}
