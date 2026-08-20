using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer.Codecs.Text;

/// <summary>编码严格 ASCII 字符串，并写入可配置宽度的字节长度前缀。</summary>
public sealed class LengthPrefixedAsciiStringValueCodec : IValueCodec<string>
{
    /// <summary>获取标准注册名称。</summary>
    public const string CodecName = "ascii";
    /// <inheritdoc />
    public void Encode(BufferWriter writer, string value, SerializationContext context, ValueCodecOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        context.ValidateStringLength(value.Length, typeof(string));
        var bytes = AsciiEncoding.Encode(value);
        writer.WriteLength(bytes.Length, options.LengthPrefixSize);
        writer.Write(bytes);
    }

    /// <inheritdoc />
    public string Decode(ref BufferReader reader, SerializationContext context, ValueCodecOptions options)
    {
        var length = reader.ReadLength(options.LengthPrefixSize);
        context.ValidateStringLength(length, typeof(string));
        return AsciiEncoding.Decode(reader.ReadSpan(length));
    }
}
