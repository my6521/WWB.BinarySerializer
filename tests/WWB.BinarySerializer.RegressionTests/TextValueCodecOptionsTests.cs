using WWB.BinarySerializer.Attributes;
using WWB.BinarySerializer.Codecs.Text;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class TextValueCodecOptionsTests
{
    [Fact]
    public void LengthPrefixedTextCodecs_UseFieldPrefixSizesFromOneThroughFour()
    {
        var runtime = new SerializerBuilder().AddTextCodecs().Build();
        var value = new TextPrefixSizesContract();

        var bytes = runtime.Serialize(value);

        Assert.Equal(new byte[]
        {
            1, 0x41,
            1, 0, 0x41,
            1, 0, 0, 0x41,
            1, 0, 0, 0, 0x41,
            1, 0xAB,
            1, 0, 0xAB,
            1, 0, 0, 0xAB,
            1, 0, 0, 0, 0xAB
        }, bytes);
        Assert.Equivalent(value, runtime.Deserialize<TextPrefixSizesContract>(bytes));
    }

    [Fact]
    public void FixedLengthTextCodec_RequiresFieldFixedLength()
    {
        var runtime = new SerializerBuilder().AddAsciiCodecs().Build();

        Assert.Throws<InvalidOperationException>(() =>
            runtime.Serialize(new MissingFixedLengthAsciiContract { Value = "A" }));
    }
}

[BinaryContract]
public sealed class TextPrefixSizesContract
{
    [BinaryField(1, LengthPrefixSize = 1, ValueCodecName = LengthPrefixedAsciiStringValueCodec.CodecName)]
    public string Ascii1 { get; set; } = "A";

    [BinaryField(2, LengthPrefixSize = 2, ValueCodecName = LengthPrefixedAsciiStringValueCodec.CodecName)]
    public string Ascii2 { get; set; } = "A";

    [BinaryField(3, LengthPrefixSize = 3, ValueCodecName = LengthPrefixedAsciiStringValueCodec.CodecName)]
    public string Ascii3 { get; set; } = "A";

    [BinaryField(4, LengthPrefixSize = 4, ValueCodecName = LengthPrefixedAsciiStringValueCodec.CodecName)]
    public string Ascii4 { get; set; } = "A";

    [BinaryField(5, LengthPrefixSize = 1, ValueCodecName = LengthPrefixedHexStringValueCodec.CodecName)]
    public string Hex1 { get; set; } = "AB";

    [BinaryField(6, LengthPrefixSize = 2, ValueCodecName = LengthPrefixedHexStringValueCodec.CodecName)]
    public string Hex2 { get; set; } = "AB";

    [BinaryField(7, LengthPrefixSize = 3, ValueCodecName = LengthPrefixedHexStringValueCodec.CodecName)]
    public string Hex3 { get; set; } = "AB";

    [BinaryField(8, LengthPrefixSize = 4, ValueCodecName = LengthPrefixedHexStringValueCodec.CodecName)]
    public string Hex4 { get; set; } = "AB";
}

[BinaryContract]
public sealed class MissingFixedLengthAsciiContract
{
    [BinaryField(1, ValueCodecName = FixedLengthAsciiStringValueCodec.CodecName)]
    public string Value { get; set; } = string.Empty;
}
