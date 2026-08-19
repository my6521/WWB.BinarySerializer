using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class CharTests
{
    [Fact]
    public void RoundTrip_UsesConfiguredBigEndian()
    {
        var bytes = BinarySerializer.SerializeObject(new CharContract { Value = '\u4E2D' });
        Assert.Equal(new byte[] { 0x4E, 0x2D }, bytes);
        Assert.Equal('\u4E2D', BinarySerializer.DeserializeObject<CharContract>(bytes).Value);
    }
}

[BinaryContract(EndianType = EndianType.Big)]
public class CharContract { [BinaryField(1)] public char Value { get; set; } }
