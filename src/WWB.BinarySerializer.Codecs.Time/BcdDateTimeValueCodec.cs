using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer.Codecs.Time;

/// <summary>按 yyyyMMddHHmmss 顺序将 DateTime 编码为 7 个压缩 BCD 字节。</summary>
public sealed class BcdDateTimeValueCodec : IValueCodec<DateTime>
{
    /// <summary>获取标准注册名称。</summary>
    public const string CodecName = "bcd-datetime";

    /// <inheritdoc />
    public void Encode(BufferWriter writer, DateTime value, SerializationContext context, ValueCodecOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        Span<byte> bytes = stackalloc byte[7];
        bytes[0] = PackedBcd.Encode(value.Year / 100, nameof(value));
        bytes[1] = PackedBcd.Encode(value.Year % 100, nameof(value));
        bytes[2] = PackedBcd.Encode(value.Month, nameof(value));
        bytes[3] = PackedBcd.Encode(value.Day, nameof(value));
        bytes[4] = PackedBcd.Encode(value.Hour, nameof(value));
        bytes[5] = PackedBcd.Encode(value.Minute, nameof(value));
        bytes[6] = PackedBcd.Encode(value.Second, nameof(value));
        writer.Write(bytes);
    }

    /// <inheritdoc />
    public DateTime Decode(ref BufferReader reader, SerializationContext context, ValueCodecOptions options)
    {
        var bytes = reader.ReadSpan(7);
        var year = PackedBcd.Decode(bytes[0]) * 100 + PackedBcd.Decode(bytes[1]);
        var month = PackedBcd.Decode(bytes[2]);
        var day = PackedBcd.Decode(bytes[3]);
        var hour = PackedBcd.Decode(bytes[4]);
        var minute = PackedBcd.Decode(bytes[5]);
        var second = PackedBcd.Decode(bytes[6]);

        try
        {
            return new DateTime(year, month, day, hour, minute, second);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new FormatException("The payload is not a valid packed BCD date and time.", exception);
        }
    }
}
