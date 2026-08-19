using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;
using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Codecs.Time;
using WWB.BinarySerializer.Runtime;
using Xunit;

namespace WWB.BinarySerializer.RegressionTests;

public class Cp56Time2aValueCodecTests
{
    [Fact]
    public void RoundTrip_Cp56Time2a_UsesSevenByteWireFormat()
    {
        var value = new DateTime(2024, 2, 3, 4, 5, 6);
        var runtime = new SerializerBuilder().AddTimeCodecs().Build();
        var bytes = runtime.Serialize(new Cp56Time2aContract { Value = value });

        Assert.Equal(new byte[] { 0x70, 0x17, 0x05, 0x04, 0x03, 0x02, 0x18 }, bytes);
        Assert.Equal(value, runtime.Deserialize<Cp56Time2aContract>(bytes).Value);
    }

    [Fact]
    public void RoundTrip_Cp56Time2a_PreservesMilliseconds()
    {
        var value = new DateTime(2099, 12, 31, 23, 59, 58, 321);
        var runtime = new SerializerBuilder().AddValueCodec(Cp56Time2aValueCodec.CodecName, new Cp56Time2aValueCodec()).Build();

        Assert.Equal(value, runtime.Deserialize<Cp56Time2aContract>(runtime.Serialize(new Cp56Time2aContract { Value = value })).Value);
    }

    [Fact]
    public void Serialize_Cp56Time2a_RejectsUnsupportedYear()
    {
        var runtime = new SerializerBuilder().AddValueCodec(Cp56Time2aValueCodec.CodecName, new Cp56Time2aValueCodec()).Build();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            runtime.Serialize(new Cp56Time2aContract { Value = new DateTime(1999, 12, 31) }));
    }

    [Fact]
    public void Deserialize_Cp56Time2a_RejectsInvalidPayload()
    {
        var runtime = new SerializerBuilder().AddValueCodec(Cp56Time2aValueCodec.CodecName, new Cp56Time2aValueCodec()).Build();

        Assert.Throws<FormatException>(() =>
            runtime.Deserialize<Cp56Time2aContract>(new byte[] { 0, 0, 0, 0, 0, 0, 0 }));
    }

    [Fact]
    public void Deserialize_Cp56Time2a_RejectsInvalidOrUnsupportedStatusBits()
    {
        var runtime = new SerializerBuilder().AddTimeCodecs().Build();

        Assert.Throws<FormatException>(() =>
            runtime.Deserialize<Cp56Time2aContract>(new byte[] { 0, 0, 0x80, 0, 1, 1, 24 }));
    }

    [Fact]
    public void Contract_CanUseDefaultAndMultipleNamedDateTimeCodecs()
    {
        var ordinary = new DateTime(2026, 8, 20, 10, 11, 12, DateTimeKind.Utc);
        var cp56 = new DateTime(2024, 2, 3, 4, 5, 6, 789);
        var bcd = new DateTime(2030, 9, 8, 7, 6, 5);
        var runtime = new SerializerBuilder()
            .AddValueCodec(Cp56Time2aValueCodec.CodecName, new Cp56Time2aValueCodec())
            .AddValueCodec(BcdDateTimeValueCodec.CodecName, new BcdDateTimeValueCodec())
            .Build();

        var payload = runtime.Serialize(new MixedDateTimeContract
        {
            Ordinary = ordinary,
            Cp56 = cp56,
            Bcd = bcd
        });
        var result = runtime.Deserialize<MixedDateTimeContract>(payload);

        Assert.Equal(22, payload.Length);
        Assert.Equal(ordinary, result.Ordinary);
        Assert.Equal(cp56, result.Cp56);
        Assert.Equal(bcd, result.Bcd);
    }
}

[BinaryContract]
public class Cp56Time2aContract
{
    [BinaryField(1, ValueCodecName = Cp56Time2aValueCodec.CodecName)]
    public DateTime Value { get; set; }
}

[BinaryContract]
public class MixedDateTimeContract
{
    [BinaryField(1)]
    public DateTime Ordinary { get; set; }

    [BinaryField(2, ValueCodecName = Cp56Time2aValueCodec.CodecName)]
    public DateTime Cp56 { get; set; }

    [BinaryField(3, ValueCodecName = BcdDateTimeValueCodec.CodecName)]
    public DateTime Bcd { get; set; }
}
