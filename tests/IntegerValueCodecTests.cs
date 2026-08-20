using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class IntegerValueCodecTests
{
    [Fact]
    public void BuiltInIntegerValueCodecs_UseExactWireFormats()
    {
        var runtime = new SerializerBuilder().Build();
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
        var runtime = new SerializerBuilder().Build();
        var value = new UInt8ListContract { Values = new List<int> { 0, 127, 255 } };

        var bytes = runtime.Serialize(value);

        Assert.Equal(new byte[] { 3, 0, 127, 255 }, bytes);
        Assert.Equal(value.Values, runtime.Deserialize<UInt8ListContract>(bytes).Values);
    }

    [Fact]
    public void BuiltInIntegerValueCodecs_PreserveBoundaryValues()
    {
        var runtime = new SerializerBuilder().Build();
        var value = new IntegerWireContract
        {
            UInt8 = 0,
            Int8 = 127,
            UInt16LittleEndian = 65535,
            UInt16BigEndian = 65535,
            Int16LittleEndian = -32768,
            Int16BigEndian = -32768,
            UInt24LittleEndian = 16777215,
            UInt24BigEndian = 16777215,
            Int24LittleEndian = -8388608,
            Int24BigEndian = -8388608
        };

        var bytes = runtime.Serialize(value);

        Assert.Equal(new byte[]
        {
            0x00, 0x7F,
            0xFF, 0xFF, 0xFF, 0xFF,
            0x00, 0x80, 0x80, 0x00,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0x00, 0x00, 0x80, 0x80, 0x00, 0x00
        }, bytes);
        Assert.Equivalent(value, runtime.Deserialize<IntegerWireContract>(bytes));
    }

    [Fact]
    public void ExplicitIntegerEndianness_OverridesContractEndianness()
    {
        var runtime = new SerializerBuilder().Build();
        var value = new BigEndianIntegerWireContract
        {
            LittleEndian = 0x1234,
            BigEndian = 0x1234
        };

        var bytes = runtime.Serialize(value);

        Assert.Equal(new byte[] { 0x34, 0x12, 0x12, 0x34 }, bytes);
        Assert.Equivalent(value, runtime.Deserialize<BigEndianIntegerWireContract>(bytes));
    }

    [Fact]
    public void ReplacingBuiltInCodec_IsIsolatedToCurrentRuntime()
    {
        var replaced = new SerializerBuilder()
            .ReplaceValueCodec(Int32WireCodecs.UInt8, new OffsetValueCodec(10))
            .Build();
        var standard = new SerializerBuilder().Build();

        Assert.Equal(new byte[] { 11, 0, 0, 0 }, replaced.Serialize(new UInt8Contract { Value = 1 }));
        Assert.Equal(new byte[] { 1 }, standard.Serialize(new UInt8Contract { Value = 1 }));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(256)]
    public void UInt8Codec_RejectsOutOfRangeValues(int value)
    {
        var runtime = new SerializerBuilder().Build();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            runtime.Serialize(new UInt8Contract { Value = value }));
    }

    [Fact]
    public void Int24Codec_RejectsTruncatedPayload()
    {
        var runtime = new SerializerBuilder().Build();

        Assert.Throws<SerializationException>(() =>
            runtime.Deserialize<Int24Contract>(new byte[] { 0x01, 0x02 }));
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

[BinaryContract(EndianType = EndianType.Big)]
public sealed class BigEndianIntegerWireContract
{
    [BinaryField(1, ValueCodecName = Int32WireCodecs.UInt16LittleEndian)]
    public int LittleEndian { get; set; }

    [BinaryField(2, ValueCodecName = Int32WireCodecs.UInt16BigEndian)]
    public int BigEndian { get; set; }
}
