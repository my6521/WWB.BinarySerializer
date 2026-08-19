using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer.Codecs.Text;

/// <summary>使用指定且无前缀的字节长度编码严格 ASCII 字符串。</summary>
public sealed class FixedLengthAsciiStringValueCodec : IValueCodec<string>
{
    private readonly int _length;

    /// <summary>使用指定字节长度初始化无前缀 ASCII Codec。</summary>
    public FixedLengthAsciiStringValueCodec(int length)
    {
        if (length < 1)
            throw new ArgumentOutOfRangeException(nameof(length), length, "Fixed ASCII length must be greater than zero.");
        _length = length;
    }

    /// <inheritdoc />
    public void Encode(BufferWriter writer, string value, SerializationContext context)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var bytes = AsciiEncoding.Encode(value);
        context.ValidateStringLength(bytes.Length, typeof(string));
        if (bytes.Length != _length)
            throw new ArgumentException($"ASCII value must contain exactly {_length} bytes.", nameof(value));
        writer.Write(bytes);
    }

    /// <inheritdoc />
    public string Decode(ref BufferReader reader, SerializationContext context)
    {
        context.ValidateStringLength(_length, typeof(string));
        return AsciiEncoding.Decode(reader.ReadSpan(_length));
    }
}
