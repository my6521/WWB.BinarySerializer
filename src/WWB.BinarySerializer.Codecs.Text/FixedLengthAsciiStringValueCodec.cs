using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer.Codecs.Text;

/// <summary>使用字段配置的固定且无前缀字节长度编码严格 ASCII 字符串。</summary>
public sealed class FixedLengthAsciiStringValueCodec : IValueCodec<string>
{
    /// <summary>获取固定长度 ASCII Codec 的稳定注册名称。</summary>
    public const string CodecName = "ascii-fixed";

    /// <inheritdoc />
    public void Encode(BufferWriter writer, string value, SerializationContext context, ValueCodecOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        var length = GetFixedLength(options);
        context.ValidateStringLength(value.Length, typeof(string));
        if (value.Length != length)
            throw new ArgumentException($"ASCII value must contain exactly {length} bytes.", nameof(value));
        var bytes = AsciiEncoding.Encode(value);
        writer.Write(bytes);
    }

    /// <inheritdoc />
    public string Decode(ref BufferReader reader, SerializationContext context, ValueCodecOptions options)
    {
        var length = GetFixedLength(options);
        context.ValidateStringLength(length, typeof(string));
        return AsciiEncoding.Decode(reader.ReadSpan(length));
    }

    private static int GetFixedLength(ValueCodecOptions options) =>
        options.FixedLength > 0
            ? options.FixedLength
            : throw new InvalidOperationException("Fixed-length ASCII Codec requires BinaryField.FixedLength.");
}
