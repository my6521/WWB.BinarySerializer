using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer.Codecs.Time;

/// <summary>
/// 使用 IEC 60870-5 的 7 字节 CP56Time2a 格式编码 <see cref="DateTime"/> 值。
/// </summary>
public sealed class Cp56Time2aValueCodec : IValueCodec<DateTime>
{
    /// <summary>获取标准注册名称。</summary>
    public const string CodecName = "cp56time2a";

    /// <inheritdoc />
    public void Encode(BufferWriter writer, DateTime value, SerializationContext context)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value.Year is < 2000 or > 2099)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "CP56Time2a supports years from 2000 through 2099.");
        }

        var milliseconds = value.Second * 1000 + value.Millisecond;
        Span<byte> bytes = stackalloc byte[7];
        bytes[0] = (byte)milliseconds;
        bytes[1] = (byte)(milliseconds >> 8);
        bytes[2] = (byte)value.Minute;
        bytes[3] = (byte)value.Hour;
        bytes[4] = (byte)value.Day;
        bytes[5] = (byte)value.Month;
        bytes[6] = (byte)(value.Year - 2000);
        writer.Write(bytes);
    }

    /// <inheritdoc />
    public DateTime Decode(ref BufferReader reader, SerializationContext context)
    {
        var bytes = reader.ReadSpan(7);
        var milliseconds = bytes[0] | (bytes[1] << 8);
        if ((bytes[2] & 0xC0) != 0 || (bytes[3] & 0xE0) != 0 ||
            (bytes[5] & 0xF0) != 0 || (bytes[6] & 0x80) != 0)
        {
            throw new FormatException("The CP56Time2a payload contains unsupported status or reserved bits.");
        }
        var minute = bytes[2] & 0x3F;
        var hour = bytes[3] & 0x1F;
        var day = bytes[4] & 0x1F;
        var month = bytes[5] & 0x0F;
        var year = 2000 + (bytes[6] & 0x7F);

        if (milliseconds >= 60_000 || minute > 59 || hour > 23 || month is < 1 or > 12 ||
            day < 1 || day > DateTime.DaysInMonth(year, month))
        {
            throw new FormatException("The payload is not a valid CP56Time2a value.");
        }

        return new DateTime(year, month, day, hour, minute, milliseconds / 1000, milliseconds % 1000);
    }
}
