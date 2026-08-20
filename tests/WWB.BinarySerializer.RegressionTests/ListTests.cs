using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class ListTests
{
    [Fact]
    public void Contract_HasCompileTimeRegisteredCodec()
    {
        Assert.NotNull(GeneratedCodecRegistry<IntListContract>.Instance);
    }

    [Fact]
    public void CollectionValueCodec_IsResolvedFromRuntime()
    {
        var source = new HandlerListContract { Values = new List<int> { 1, 2 } };
        var runtime = new SerializerBuilder().AddValueCodec("offset", new OffsetValueCodec(10)).Build();
        var bytes = runtime.Serialize(source);
        Assert.Equal(new byte[] { 2, 11, 0, 0, 0, 12, 0, 0, 0 }, bytes);
        Assert.Equal(source.Values, runtime.Deserialize<HandlerListContract>(bytes).Values);
    }

    [Fact]
    public void RoundTrip_VariablePrimitiveList_PreservesElementCountAndValues()
    {
        var bytes = BinarySerializer.SerializeObject(new IntListContract { Values = new List<int> { 1, -2, 3 } });

        Assert.Equal(new byte[] { 3, 1, 0, 0, 0, 254, 255, 255, 255, 3, 0, 0, 0 }, bytes);
        Assert.Equal(new[] { 1, -2, 3 }, BinarySerializer.DeserializeObject<IntListContract>(bytes).Values);
    }
}

[BinaryContract]
public class HandlerListContract
{
    [BinaryField(1, ValueCodecName = "offset")]
    public List<int> Values { get; set; } = new();
}

[BinaryContract]
public class IntListContract
{
    [BinaryField(1)]
    public List<int> Values { get; set; } = new();
}
