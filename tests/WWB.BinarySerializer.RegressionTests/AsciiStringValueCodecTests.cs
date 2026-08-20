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
        var runtime = new SerializerBuilder().AddAsciiCodecs().Build();

        var payload = runtime.Serialize(new AsciiStringContract { Value = "ABC" });

        Assert.Equal(new byte[] { 3, (byte)'A', (byte)'B', (byte)'C' }, payload);
        Assert.Equal("ABC", runtime.Deserialize<AsciiStringContract>(payload).Value);
    }

    [Fact]
    public void RoundTrip_FixedLengthAscii_HasNoPrefix()
    {
        var runtime = new SerializerBuilder().AddAsciiCodecs().Build();

        var payload = runtime.Serialize(new FixedAsciiStringContract { Value = "ABC" });

        Assert.Equal(new byte[] { (byte)'A', (byte)'B', (byte)'C' }, payload);
        Assert.Equal("ABC", runtime.Deserialize<FixedAsciiStringContract>(payload).Value);
    }

    [Fact]
    public void Serialize_Ascii_RejectsNonAsciiCharacters()
    {
        var runtime = new SerializerBuilder().AddAsciiCodecs().Build();

        Assert.Throws<EncoderFallbackException>(() =>
            runtime.Serialize(new AsciiStringContract { Value = "设备" }));
    }

    [Fact]
    public void Deserialize_Ascii_RejectsHighBitBytes()
    {
        var runtime = new SerializerBuilder().AddAsciiCodecs().Build();

        Assert.Throws<DecoderFallbackException>(() =>
            runtime.Deserialize<AsciiStringContract>(new byte[] { 1, 0x80 }));
    }

    [Fact]
    public void FixedLengthAsciiCodecs_WithDifferentLengthsCanCoexist()
    {
        var runtime = new SerializerBuilder().AddAsciiCodecs().Build();
        var value = new MultipleFixedAsciiContract { Short = "AB", Long = "XYZ" };

        var bytes = runtime.Serialize(value);

        Assert.Equal(new byte[] { 0x41, 0x42, 0x58, 0x59, 0x5A }, bytes);
        Assert.Equivalent(value, runtime.Deserialize<MultipleFixedAsciiContract>(bytes));
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
    [BinaryField(1, FixedLength = 3, ValueCodecName = FixedLengthAsciiStringValueCodec.CodecName)]
    public string Value { get; set; } = string.Empty;
}

[BinaryContract]
public sealed class MultipleFixedAsciiContract
{
    [BinaryField(1, FixedLength = 2, ValueCodecName = FixedLengthAsciiStringValueCodec.CodecName)]
    public string Short { get; set; } = string.Empty;

    [BinaryField(2, FixedLength = 3, ValueCodecName = FixedLengthAsciiStringValueCodec.CodecName)]
    public string Long { get; set; } = string.Empty;
}
