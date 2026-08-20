using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class IntegerValueCodecTests
{
    [Fact]
    public void AddIntegerValueCodecs_UsesExactWireFormats()
    {
        var runtime = new SerializerBuilder().AddIntegerValueCodecs().Build();
        var value = new IntegerWireContract
        {
            UInt8 = 255,
            Int8 = -128,
            UInt16LittleEndian = 0x1234,
            UInt16BigEndian = 0x1234,
            Int16LittleEndian = -2,
            Int16BigEndian = -2,
            UInt24LittleEndian = 0x123456,
            UInt24BigEndian = 0x123456,
            Int24LittleEndian = -2,
            Int24BigEndian = -2
        };

        var bytes = runtime.Serialize(value);

        Assert.Equal(new byte[]
        {
            0xFF, 0x80,
            0x34, 0x12, 0x12, 0x34,
            0xFE, 0xFF, 0xFF, 0xFE,
            0x56, 0x34, 0x12, 0x12, 0x34, 0x56,
            0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE
        }, bytes);
        Assert.Equivalent(value, runtime.Deserialize<IntegerWireContract>(bytes));
    }

    [Fact]
    public void UInt8Codec_IsAppliedToEveryListElement()
    {
        var runtime = new SerializerBuilder().AddIntegerValueCodecs().Build();
        var value = new UInt8ListContract { Values = new List<int> { 0, 127, 255 } };

        var bytes = runtime.Serialize(value);

        Assert.Equal(new byte[] { 3, 0, 127, 255 }, bytes);
        Assert.Equal(value.Values, runtime.Deserialize<UInt8ListContract>(bytes).Values);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(256)]
    public void UInt8Codec_RejectsOutOfRangeValues(int value)
    {
        var runtime = new SerializerBuilder().AddIntegerValueCodecs().Build();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            runtime.Serialize(new UInt8Contract { Value = value }));
    }

    [Fact]
    public void Int24Codec_RejectsTruncatedPayload()
    {
        var runtime = new SerializerBuilder().AddIntegerValueCodecs().Build();

        Assert.Throws<SerializationException>(() =>
            runtime.Deserialize<Int24Contract>(new byte[] { 0x01, 0x02 }));
    }

    [Fact]
    public void IntegerCodec_MustBeRegistered()
    {
        var runtime = new SerializerBuilder().Build();

        Assert.Throws<CodecNotFoundException>(() =>
            runtime.Serialize(new UInt8Contract { Value = 1 }));
    }
}

[BinaryContract]
public sealed class IntegerWireContract
{
    [BinaryField(1, ValueCodecName = Int32WireCodecs.UInt8)] public int UInt8 { get; set; }
    [BinaryField(2, ValueCodecName = Int32WireCodecs.Int8)] public int Int8 { get; set; }
    [BinaryField(3, ValueCodecName = Int32WireCodecs.UInt16LittleEndian)] public int UInt16LittleEndian { get; set; }
    [BinaryField(4, ValueCodecName = Int32WireCodecs.UInt16BigEndian)] public int UInt16BigEndian { get; set; }
    [BinaryField(5, ValueCodecName = Int32WireCodecs.Int16LittleEndian)] public int Int16LittleEndian { get; set; }
    [BinaryField(6, ValueCodecName = Int32WireCodecs.Int16BigEndian)] public int Int16BigEndian { get; set; }
    [BinaryField(7, ValueCodecName = Int32WireCodecs.UInt24LittleEndian)] public int UInt24LittleEndian { get; set; }
    [BinaryField(8, ValueCodecName = Int32WireCodecs.UInt24BigEndian)] public int UInt24BigEndian { get; set; }
    [BinaryField(9, ValueCodecName = Int32WireCodecs.Int24LittleEndian)] public int Int24LittleEndian { get; set; }
    [BinaryField(10, ValueCodecName = Int32WireCodecs.Int24BigEndian)] public int Int24BigEndian { get; set; }
}

[BinaryContract]
public sealed class UInt8Contract
{
    [BinaryField(1, ValueCodecName = Int32WireCodecs.UInt8)]
    public int Value { get; set; }
}

[BinaryContract]
public sealed class Int24Contract
{
    [BinaryField(1, ValueCodecName = Int32WireCodecs.Int24LittleEndian)]
    public int Value { get; set; }
}

[BinaryContract]
public sealed class UInt8ListContract
{
    [BinaryField(1, ValueCodecName = Int32WireCodecs.UInt8)]
    public List<int> Values { get; set; } = new();
}
