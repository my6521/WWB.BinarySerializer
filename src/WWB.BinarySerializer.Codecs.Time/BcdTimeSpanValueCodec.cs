using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer.Codecs.Time;

/// <summary>按 HHmm 顺序将非负日内时间编码为 2 个压缩 BCD 字节。</summary>
public sealed class BcdTimeSpanValueCodec : IValueCodec<TimeSpan>
{
    /// <summary>获取标准注册名称。</summary>
    public const string CodecName = "bcd-timespan";

    /// <inheritdoc />
    public void Encode(BufferWriter writer, TimeSpan value, SerializationContext context)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (value < TimeSpan.Zero || value >= TimeSpan.FromDays(1))
            throw new ArgumentOutOfRangeException(nameof(value), value, "BCD time values must be within one day.");

        Span<byte> bytes = stackalloc byte[2];
        bytes[0] = PackedBcd.Encode(value.Hours, nameof(value));
        bytes[1] = PackedBcd.Encode(value.Minutes, nameof(value));
        writer.Write(bytes);
    }

    /// <inheritdoc />
    public TimeSpan Decode(ref BufferReader reader, SerializationContext context)
    {
        var bytes = reader.ReadSpan(2);
        var hours = PackedBcd.Decode(bytes[0]);
        var minutes = PackedBcd.Decode(bytes[1]);
        if (hours > 23 || minutes > 59)
            throw new FormatException("The payload is not a valid packed BCD time-of-day value.");
        return new TimeSpan(hours, minutes, 0);
    }
}
