using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer.Codecs.Time;

/// <summary>将 DateTime 值编码为以秒为单位的无符号 32 位 Unix 时间戳。</summary>
public sealed class UnixTimeSecondsValueCodec : IValueCodec<DateTime>
{
    /// <summary>获取标准注册名称。</summary>
    public const string CodecName = "unix-time-seconds";

    /// <inheritdoc />
    public void Encode(BufferWriter writer, DateTime value, SerializationContext context, ValueCodecOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteUInt32(UnixTime.ToUInt32Seconds(value));
    }

    /// <inheritdoc />
    public DateTime Decode(ref BufferReader reader, SerializationContext context, ValueCodecOptions options) =>
        UnixTime.FromUInt32Seconds(reader.ReadUInt32());
}
