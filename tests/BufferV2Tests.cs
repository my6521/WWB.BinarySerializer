using WWB.BinarySerializer.Buffers;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class BufferV2Tests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NumericPrimitives_RoundTripInBothEndianModes(bool bigEndian)
    {
        var writer = new BufferWriter(bigEndian: bigEndian);
        writer.WriteUInt16(0xFEDC);
        writer.WriteUInt32(0xFEDCBA98);
        writer.WriteUInt64(0xFEDCBA9876543210);
        writer.WriteSingle(12.5f);
        writer.WriteDouble(-42.25);

        var reader = new BufferReader(writer.WrittenSpan, bigEndian);
        Assert.Equal((ushort)0xFEDC, reader.ReadUInt16());
        Assert.Equal(0xFEDCBA98u, reader.ReadUInt32());
        Assert.Equal(0xFEDCBA9876543210ul, reader.ReadUInt64());
        Assert.Equal(12.5f, reader.ReadSingle());
        Assert.Equal(-42.25, reader.ReadDouble());
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void SpanDeserialize_UsesNativeCodec()
    {
        var runtime = new SerializerBuilder().AddCodec(new NativeRuntimeCodec()).Build();
        ReadOnlySpan<byte> data = new byte[] { 4, 3, 2, 1 };

        var result = runtime.Deserialize<NativeRuntimeContract>(data);

        Assert.Equal(0x01020304, result.Value);
    }

    [Fact]
    public void GeneratedCodecRegistry_ConcurrentRegistrationPublishesExactlyOneCodec()
    {
        var codecs = Enumerable.Range(0, 32).Select(_ => new ConcurrentCodec()).ToArray();

        var results = codecs.AsParallel().Select(GeneratedCodecRegistry<ConcurrentContract>.TryRegister).ToArray();

        Assert.Single(results, value => value);
        Assert.Contains(GeneratedCodecRegistry<ConcurrentContract>.Instance, codecs);
    }

    [Theory]
    [InlineData(false, new byte[] { 0, 0, 0, 128 })]
    [InlineData(true, new byte[] { 128, 0, 0, 0 })]
    public void FourByteLengthPrefix_RejectsValuesAboveInt32(bool bigEndian, byte[] data)
    {
        var exception = Assert.Throws<SerializationException>(() => ReadFourByteLength(data, bigEndian));

        Assert.Equal(0, exception.Offset);
    }

    private static int ReadFourByteLength(byte[] data, bool bigEndian)
    {
        var reader = new BufferReader(data, bigEndian);
        return reader.ReadLength(4);
    }
}

public sealed class ConcurrentContract { }

internal sealed class ConcurrentCodec : IBinaryCodec<ConcurrentContract>
{
    public void Encode(BufferWriter writer, ConcurrentContract value, SerializationContext context) { }
    public ConcurrentContract Decode(ref BufferReader reader, SerializationContext context) => new();
}
