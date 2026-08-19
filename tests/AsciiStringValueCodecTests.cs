using System.Text;
using WWB.BinarySerializer.Attributes;
using WWB.BinarySerializer.Codecs.Text;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class AsciiStringValueCodecTests
{
    [Fact]
    public void RoundTrip_LengthPrefixedAscii_UsesByteLengthPrefix()
    {
        var runtime = new SerializerBuilder().AddAsciiCodec().Build();

        var payload = runtime.Serialize(new AsciiStringContract { Value = "ABC" });

        Assert.Equal(new byte[] { 3, (byte)'A', (byte)'B', (byte)'C' }, payload);
        Assert.Equal("ABC", runtime.Deserialize<AsciiStringContract>(payload).Value);
    }

    [Fact]
    public void RoundTrip_FixedLengthAscii_HasNoPrefix()
    {
        var runtime = new SerializerBuilder()
            .AddValueCodec("ascii-3", new FixedLengthAsciiStringValueCodec(3))
            .Build();

        var payload = runtime.Serialize(new FixedAsciiStringContract { Value = "ABC" });

        Assert.Equal(new byte[] { (byte)'A', (byte)'B', (byte)'C' }, payload);
        Assert.Equal("ABC", runtime.Deserialize<FixedAsciiStringContract>(payload).Value);
    }

    [Fact]
    public void Serialize_Ascii_RejectsNonAsciiCharacters()
    {
        var runtime = new SerializerBuilder().AddAsciiCodec().Build();

        Assert.Throws<EncoderFallbackException>(() =>
            runtime.Serialize(new AsciiStringContract { Value = "设备" }));
    }

    [Fact]
    public void Deserialize_Ascii_RejectsHighBitBytes()
    {
        var runtime = new SerializerBuilder().AddAsciiCodec().Build();

        Assert.Throws<DecoderFallbackException>(() =>
            runtime.Deserialize<AsciiStringContract>(new byte[] { 1, 0x80 }));
    }
}

[BinaryContract]
public sealed class AsciiStringContract
{
    [BinaryField(1, ValueCodecName = LengthPrefixedAsciiStringValueCodec.CodecName)]
    public string Value { get; set; } = string.Empty;
}

[BinaryContract]
public sealed class FixedAsciiStringContract
{
    [BinaryField(1, ValueCodecName = "ascii-3")]
    public string Value { get; set; } = string.Empty;
}
