using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using WWB.BinarySerializer.Buffers;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class GeneratedComplexCodecTests
{
    [Fact]
    public void GeneratedNestedCodec_EnforcesRuntimeDepthLimit()
    {
        var runtime = new SerializerBuilder()
            .Configure(new SerializerOptions { MaxDepth = 1 })
            .Build();

        Assert.Throws<SerializationException>(() => runtime.Serialize(
            new GeneratedParentContract { Child = new GeneratedChildContract() }));
    }

    [Fact]
    public void GeneratedCodec_IsRegisteredForBothRuntimeGenerations()
    {
        Assert.NotNull(GeneratedCodecRegistry<GeneratedArrayContract>.Instance);
    }

    [Fact]
    public void ArrayContract_HasGeneratedCodec()
    {
        Assert.NotNull(GeneratedCodecRegistry<GeneratedArrayContract>.Instance);
    }

    [Fact]
    public void NestedContractAndChild_HaveGeneratedCodecs()
    {
        Assert.NotNull(GeneratedCodecRegistry<GeneratedParentContract>.Instance);
        Assert.NotNull(GeneratedCodecRegistry<GeneratedChildContract>.Instance);
    }

    [Fact]
    public void ArrayElementValueCodec_IsResolvedFromRuntime()
    {
        var source = new OffsetArrayContract { Values = new[] { 1, 2 } };
        var runtime = new SerializerBuilder().AddValueCodec("offset", new OffsetValueCodec(10)).Build();
        var bytes = runtime.Serialize(source);
        Assert.Equal(new byte[] { 2, 11, 0, 0, 0, 12, 0, 0, 0 }, bytes);
        Assert.Equal(source.Values, runtime.Deserialize<OffsetArrayContract>(bytes).Values);
    }

    [Fact]
    public void NestedCodec_IsResolvedFromEachRuntimeSnapshot()
    {
        var first = new SerializerBuilder().AddCodec(new ConfigurableChildCodec(10)).Build();
        var second = new SerializerBuilder().AddCodec(new ConfigurableChildCodec(20)).Build();
        var source = new GeneratedParentContract { Child = new GeneratedChildContract { Value = 1 } };

        Parallel.For(0, 100, _ =>
        {
            var firstBytes = first.Serialize(source);
            var secondBytes = second.Serialize(source);

            Assert.Equal(new byte[] { 11 }, firstBytes);
            Assert.Equal(new byte[] { 21 }, secondBytes);
            Assert.Equal(1, first.Deserialize<GeneratedParentContract>(firstBytes).Child.Value);
            Assert.Equal(1, second.Deserialize<GeneratedParentContract>(secondBytes).Child.Value);
        });
    }
}

[BinaryContract]
public class OffsetArrayContract
{
    [BinaryField(1, ValueCodecName = "offset")]
    public int[] Values { get; set; } = Array.Empty<int>();
}

[BinaryContract]
public class GeneratedArrayContract
{
    [BinaryField(1)]
    public int[] Values { get; set; } = Array.Empty<int>();
}

[BinaryContract]
public class GeneratedParentContract
{
    [BinaryField(1)]
    public GeneratedChildContract Child { get; set; } = new();
}

public class GeneratedChildContract
{
    [BinaryField(1)]
    public int Value { get; set; }
}

internal sealed class ConfigurableChildCodec : IBinaryCodec<GeneratedChildContract>
{
    private readonly int _offset;
    public ConfigurableChildCodec(int offset) => _offset = offset;

    public void Encode(BufferWriter writer, GeneratedChildContract value, SerializationContext context) =>
        writer.WriteByte(checked((byte)(value.Value + _offset)));

    public GeneratedChildContract Decode(ref BufferReader reader, SerializationContext context) =>
        new() { Value = reader.ReadByte() - _offset };
}
