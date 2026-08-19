using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Attributes;
using WWB.BinarySerializer.Runtime;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class SerializerRuntimeTests
{
    [Fact]
    public void ValueCodec_IsResolvedFromImmutableRuntimeConfiguration()
    {
        var runtime = new SerializerBuilder()
            .AddCodec(new ValueCodecContractCodec())
            .AddValueCodec("offset", new OffsetValueCodec(10))
            .Build();

        var bytes = runtime.Serialize(new ValueCodecContract { Value = 5 });
        var result = runtime.Deserialize<ValueCodecContract>(bytes);

        Assert.Equal(new byte[] { 15, 0, 0, 0 }, bytes);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public void GeneratedScalar_UsesRuntimeValueCodec()
    {
        var runtime = new SerializerBuilder()
            .AddValueCodec("offset", new OffsetValueCodec(10))
            .Build();

        var bytes = runtime.Serialize(new GeneratedValueCodecContract { Value = 5 });
        var result = runtime.Deserialize<GeneratedValueCodecContract>(bytes);

        Assert.Equal(new byte[] { 15, 0, 0, 0 }, bytes);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public void GeneratedCollection_UsesRuntimeValueCodecForEveryElement()
    {
        var runtime = new SerializerBuilder()
            .AddValueCodec("offset", new OffsetValueCodec(10))
            .Build();

        var bytes = runtime.Serialize(new GeneratedValueCodecCollectionContract { Values = new[] { 1, 2 } });
        var result = runtime.Deserialize<GeneratedValueCodecCollectionContract>(bytes);

        Assert.Equal(new byte[] { 2, 11, 0, 0, 0, 12, 0, 0, 0 }, bytes);
        Assert.Equal(new[] { 1, 2 }, result.Values);
    }

    [Fact]
    public void GeneratedContract_CanUseMultipleNamedCodecsForSameType()
    {
        var runtime = new SerializerBuilder()
            .AddValueCodec("offset-10", new OffsetValueCodec(10))
            .AddValueCodec("offset-20", new OffsetValueCodec(20))
            .Build();

        var bytes = runtime.Serialize(new MultipleNamedCodecContract { First = 1, Second = 2 });
        var result = runtime.Deserialize<MultipleNamedCodecContract>(bytes);

        Assert.Equal(new byte[] { 11, 0, 0, 0, 22, 0, 0, 0 }, bytes);
        Assert.Equal(1, result.First);
        Assert.Equal(2, result.Second);
    }

    [Fact]
    public void Build_RejectsDuplicateValueCodecNameForSameType()
    {
        var builder = new SerializerBuilder().AddValueCodec("offset", new OffsetValueCodec(10));

        Assert.Throws<InvalidOperationException>(() =>
            builder.AddValueCodec("offset", new OffsetValueCodec(20)));
    }

    [Fact]
    public void MissingNamedValueCodec_ReportsTypeAndName()
    {
        var exception = Assert.Throws<CodecNotFoundException>(() =>
            SerializerRuntime.CreateDefault().Serialize(new GeneratedValueCodecContract { Value = 1 }));

        Assert.Equal(typeof(int), exception.ContractType);
        Assert.Equal("offset", exception.CodecName);
    }

    [Fact]
    public void BufferCodec_RoundTripsThroughNativeBuffers()
    {
        var runtime = new SerializerBuilder().AddCodec(new NativeRuntimeCodec()).Build();

        var bytes = runtime.Serialize(new NativeRuntimeContract { Value = 0x01020304 });
        var result = runtime.Deserialize<NativeRuntimeContract>(bytes);

        Assert.Equal(new byte[] { 4, 3, 2, 1 }, bytes);
        Assert.Equal(0x01020304, result.Value);
    }

    [Fact]
    public void BufferReader_TruncatedPayload_ReportsOffset()
    {
        var runtime = new SerializerBuilder().AddCodec(new NativeRuntimeCodec()).Build();

        var exception = Assert.Throws<SerializationException>(
            () => runtime.Deserialize<NativeRuntimeContract>(new byte[] { 1, 2 }));

        Assert.Equal(0, exception.Offset);
        Assert.Equal(typeof(NativeRuntimeContract), exception.ContractType);
        Assert.IsType<SerializationException>(exception.InnerException);
    }

    [Fact]
    public void Deserialize_RejectsTrailingDataByDefault()
    {
        var runtime = new SerializerBuilder().AddCodec(new NativeRuntimeCodec()).Build();

        var exception = Assert.Throws<TrailingDataException>(
            () => runtime.Deserialize<NativeRuntimeContract>(new byte[] { 1, 0, 0, 0, 99 }));

        Assert.Equal(4, exception.Offset);
        Assert.Equal(1, exception.TrailingLength);
        Assert.Equal(typeof(NativeRuntimeContract), exception.ContractType);
    }

    [Fact]
    public void Deserialize_CanAllowTrailingDataForFramedTransports()
    {
        var runtime = new SerializerBuilder()
            .Configure(new SerializerOptions { RequireCompletePayload = false })
            .AddCodec(new NativeRuntimeCodec())
            .Build();

        var result = runtime.Deserialize<NativeRuntimeContract>(new byte[] { 1, 0, 0, 0, 99 });

        Assert.Equal(1, result.Value);
    }

    [Fact]
    public void GeneratedCollection_RejectsLengthOverRuntimeLimitBeforeAllocation()
    {
        var runtime = new SerializerBuilder()
            .Configure(new SerializerOptions { MaxCollectionLength = 1 })
            .Build();

        var exception = Assert.Throws<CollectionLimitExceededException>(
            () => runtime.Deserialize<GeneratedValueCodecCollectionContract>(new byte[] { 2 }));

        Assert.Equal(2, exception.ActualLength);
        Assert.Equal(1, exception.MaximumLength);
    }

    [Fact]
    public void Build_CreatesIsolatedCodecSnapshots()
    {
        var first = new SerializerBuilder().AddCodec(new RuntimeV2Codec(1)).Build();
        var second = new SerializerBuilder().AddCodec(new RuntimeV2Codec(2)).Build();

        Assert.Equal(1, first.Deserialize<RuntimeContract>(new byte[] { 1 }).Value);
        Assert.Equal(2, second.Deserialize<RuntimeContract>(new byte[] { 1 }).Value);
    }

    [Fact]
    public void BuiltRuntime_IsUnaffectedByLaterBuilderChanges()
    {
        var builder = new SerializerBuilder().AddCodec(new RuntimeV2Codec(1));
        var snapshot = builder.Build();
        builder.ReplaceCodec(new RuntimeV2Codec(2));

        Assert.Equal(1, snapshot.Deserialize<RuntimeContract>(new byte[] { 1 }).Value);
        Assert.Equal(2, builder.Build().Deserialize<RuntimeContract>(new byte[] { 1 }).Value);
    }

    [Fact]
    public void Deserialize_RejectsPayloadOverConfiguredLimit()
    {
        var runtime = new SerializerBuilder()
            .Configure(new SerializerOptions { MaxPayloadLength = 2 })
            .AddCodec(new RuntimeV2Codec(1))
            .Build();

        var exception = Assert.Throws<PayloadLimitExceededException>(
            () => runtime.Deserialize<RuntimeContract>(new byte[3]));

        Assert.Equal(3, exception.ActualLength);
        Assert.Equal(2, exception.MaximumLength);
        Assert.Equal(typeof(RuntimeContract), exception.ContractType);
    }
}

public sealed class ValueCodecContract { public int Value { get; set; } }

[BinaryContract]
public sealed class GeneratedValueCodecContract
{
    [BinaryField(1, ValueCodecName = "offset")]
    public int Value { get; set; }
}

[BinaryContract]
public sealed class GeneratedValueCodecCollectionContract
{
    [BinaryField(1, ValueCodecName = "offset")]
    public int[] Values { get; set; } = Array.Empty<int>();
}

[BinaryContract]
public sealed class MultipleNamedCodecContract
{
    [BinaryField(1, ValueCodecName = "offset-10")]
    public int First { get; set; }

    [BinaryField(2, ValueCodecName = "offset-20")]
    public int Second { get; set; }
}

internal sealed class ValueCodecContractCodec : IBinaryCodec<ValueCodecContract>
{
    public void Encode(BufferWriter writer, ValueCodecContract value, SerializationContext context) =>
        context.GetValueCodec<int>("offset").Encode(writer, value.Value, context);

    public ValueCodecContract Decode(ref BufferReader reader, SerializationContext context) =>
        new() { Value = context.GetValueCodec<int>("offset").Decode(ref reader, context) };
}

internal sealed class OffsetValueCodec : IValueCodec<int>
{
    private readonly int _offset;
    public OffsetValueCodec(int offset) => _offset = offset;
    public void Encode(BufferWriter writer, int value, SerializationContext context) => writer.WriteInt32(value + _offset);
    public int Decode(ref BufferReader reader, SerializationContext context) => reader.ReadInt32() - _offset;
}

public sealed class NativeRuntimeContract
{
    public int Value { get; set; }
}

internal sealed class NativeRuntimeCodec : IBinaryCodec<NativeRuntimeContract>
{
    public void Encode(BufferWriter writer, NativeRuntimeContract value, SerializationContext context) =>
        writer.WriteInt32(value.Value);

    public NativeRuntimeContract Decode(ref BufferReader reader, SerializationContext context) =>
        new() { Value = reader.ReadInt32() };
}

public sealed class RuntimeContract
{
    public int Value { get; set; }
}

internal sealed class RuntimeV2Codec : IBinaryCodec<RuntimeContract>
{
    private readonly int _value;
    public RuntimeV2Codec(int value) => _value = value;
    public void Encode(BufferWriter writer, RuntimeContract value, SerializationContext context) => writer.WriteByte((byte)value.Value);
    public RuntimeContract Decode(ref BufferReader reader, SerializationContext context)
    {
        reader.ReadByte();
        return new() { Value = _value };
    }
}
