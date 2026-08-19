using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using WWB.BinarySerializer.Codecs.Time;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class TimeSpanTests
{
    [Fact]
    public void RoundTrip_BcdValueCodec()
    {
        var value = new TimeSpan(23, 59, 0);
        var runtime = new SerializerBuilder().AddValueCodec(BcdTimeSpanValueCodec.CodecName, new BcdTimeSpanValueCodec()).Build();
        var bytes = runtime.Serialize(new TimeSpanContract { Value = value });

        Assert.Equal(new byte[] { 0x23, 0x59 }, bytes);
        Assert.Equal(value, runtime.Deserialize<TimeSpanContract>(bytes).Value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(24)]
    public void Serialize_BcdValueCodec_RejectsValuesOutsideOneDay(int hours)
    {
        var runtime = new SerializerBuilder().AddValueCodec(BcdTimeSpanValueCodec.CodecName, new BcdTimeSpanValueCodec()).Build();
        Assert.Throws<ArgumentOutOfRangeException>(() => runtime.Serialize(new TimeSpanContract { Value = TimeSpan.FromHours(hours) }));
    }

    [Fact]
    public void Deserialize_BcdValueCodec_RejectsInvalidTime()
    {
        var runtime = new SerializerBuilder().AddValueCodec(BcdTimeSpanValueCodec.CodecName, new BcdTimeSpanValueCodec()).Build();

        Assert.Throws<FormatException>(() => runtime.Deserialize<TimeSpanContract>(new byte[] { 0x24, 0x00 }));
    }

    [Fact]
    public void RoundTrip_DefaultTimeSpan_UsesTicks()
    {
        var source = new MissingTimeSpanHandlerContract { Value = TimeSpan.FromMilliseconds(1234) };

        var result = BinarySerializer.DeserializeObject<MissingTimeSpanHandlerContract>(BinarySerializer.SerializeObject(source));

        Assert.Equal(source.Value, result.Value);
    }
}

[BinaryContract]
public class TimeSpanContract
{
    [BinaryField(1, ValueCodecName = BcdTimeSpanValueCodec.CodecName)]
    public TimeSpan Value { get; set; }
}

[BinaryContract]
public class MissingTimeSpanHandlerContract
{
    [BinaryField(1)]
    public TimeSpan Value { get; set; }
}
