using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class NestedObjectTests
{
    [Fact]
    public void RoundTrip_NestedObject_UsesItsPropertyOrder()
    {
        var bytes = BinarySerializer.SerializeObject(new ParentContract { Child = new ChildContract { Id = 7, Value = 0x0102 } });

        Assert.Equal(new byte[] { 7, 2, 1 }, bytes);
        var result = BinarySerializer.DeserializeObject<ParentContract>(bytes);
        Assert.Equal((byte)7, result.Child.Id);
        Assert.Equal((short)0x0102, result.Child.Value);
    }
}

[BinaryContract]
public class ParentContract
{
    [BinaryField(1)]
    public ChildContract Child { get; set; } = new();
}

public class ChildContract
{
    [BinaryField(1)] public byte Id { get; set; }
    [BinaryField(2)] public short Value { get; set; }
}
