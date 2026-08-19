using WWB.BinarySerializer.Attributes;
using WWB.BinarySerializer.Codecs.Time;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class UnixTimeValueCodecTests
{
    [Fact]
    public void DirectConversion_UsesUtcUnixEpoch()
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Assert.Equal(0u, UnixTime.ToUInt32Seconds(epoch));
        Assert.Equal(epoch, UnixTime.FromUInt32Seconds(0));
        Assert.Equal(DateTimeKind.Utc, UnixTime.FromUInt32Seconds(0).Kind);
    }

    [Fact]
    public void RoundTrip_UnixTimeSeconds_UsesFourByteWireFormat()
    {
        var value = UnixTime.FromUInt32Seconds(0x01020304);
        var runtime = new SerializerBuilder().AddTimeCodecs().Build();

        var payload = runtime.Serialize(new UnixTimeContract { Value = value });

        Assert.Equal(new byte[] { 0x04, 0x03, 0x02, 0x01 }, payload);
        Assert.Equal(value, runtime.Deserialize<UnixTimeContract>(payload).Value);
    }

    [Fact]
    public void Conversion_TruncatesSubSecondPrecision()
    {
        var value = new DateTime(2026, 8, 20, 1, 2, 3, 999, DateTimeKind.Utc);

        var result = UnixTime.FromUInt32Seconds(UnixTime.ToUInt32Seconds(value));

        Assert.Equal(new DateTime(2026, 8, 20, 1, 2, 3, DateTimeKind.Utc), result);
    }

    [Fact]
    public void Conversion_RejectsValueBeforeUnixEpoch()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            UnixTime.ToUInt32Seconds(new DateTime(1969, 12, 31, 23, 59, 59, DateTimeKind.Utc)));
    }

    [Fact]
    public void Conversion_SupportsFullUInt32Range()
    {
        Assert.Equal(uint.MaxValue, UnixTime.ToUInt32Seconds(UnixTime.FromUInt32Seconds(uint.MaxValue)));
    }
}

[BinaryContract]
public sealed class UnixTimeContract
{
    [BinaryField(1, ValueCodecName = UnixTimeSecondsValueCodec.CodecName)]
    public DateTime Value { get; set; }
}
