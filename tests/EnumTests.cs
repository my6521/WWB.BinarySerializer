using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class EnumTests
{
    [Fact]
    public void Contract_HasCompileTimeRegisteredCodec()
    {
        Assert.NotNull(GeneratedCodecRegistry<EnumContract>.Instance);
    }

    [Fact]
    public void RoundTrip_ByteBackedEnum_UsesUnderlyingValue()
    {
        var bytes = BinarySerializer.SerializeObject(new EnumContract { Value = ByteBackedState.Completed });

        Assert.Equal(new byte[] { 0xFE }, bytes);
        Assert.Equal(ByteBackedState.Completed, BinarySerializer.DeserializeObject<EnumContract>(bytes).Value);
    }

    [Fact]
    public void GeneratedPrimitiveCodec_UsesNativeReaderBoundsChecks()
    {
        var runtime = SerializerRuntime.CreateDefault();

        var exception = Assert.Throws<SerializationException>(
            () => runtime.Deserialize<NativeIntContract>(new byte[] { 1 }));

        Assert.Equal(0, exception.Offset);
    }
}

public enum ByteBackedState : byte { None = 0, Completed = 0xFE }

[BinaryContract]
public class EnumContract { [BinaryField(1)] public ByteBackedState Value { get; set; } }

[BinaryContract]
public class NativeIntContract { [BinaryField(1)] public int Value { get; set; } }
