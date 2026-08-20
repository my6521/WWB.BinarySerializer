using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using WWB.BinarySerializer.Codecs.Time;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class BcdDateTimeValueCodecTests
{
    [Fact]
    public void RoundTrip_BcdDateTime_WritesPackedDecimalFields()
    {
        var value = new DateTime(2024, 2, 3, 4, 5, 6);
        var runtime = new SerializerBuilder().AddTimeCodecs().Build();
        var bytes = runtime.Serialize(new BcdDateTimeContract { Value = value });

        Assert.Equal(new byte[] { 0x20, 0x24, 0x02, 0x03, 0x04, 0x05, 0x06 }, bytes);
        Assert.Equal(value, runtime.Deserialize<BcdDateTimeContract>(bytes).Value);
    }

    [Fact]
    public void Deserialize_BcdDateTime_RejectsInvalidPackedDigit()
    {
        var runtime = new SerializerBuilder().AddTimeCodecs().Build();

        Assert.Throws<FormatException>(() =>
            runtime.Deserialize<BcdDateTimeContract>(new byte[] { 0x20, 0x24, 0x1A, 0x03, 0x04, 0x05, 0x06 }));
    }
}

[BinaryContract]
public class BcdDateTimeContract
{
    [BinaryField(1, ValueCodecName = BcdDateTimeValueCodec.CodecName)]
    public DateTime Value { get; set; }
}
