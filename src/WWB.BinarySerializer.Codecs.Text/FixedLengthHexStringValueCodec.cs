using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer.Codecs.Text;

/// <summary>将十六进制字符串编码为指定数量且不带前缀的字节。</summary>
public sealed class FixedLengthHexStringValueCodec : IValueCodec<string>
{
    private readonly int _byteLength;

    /// <summary>使用指定的二进制字节长度初始化无前缀十六进制 Codec。</summary>
    public FixedLengthHexStringValueCodec(int byteLength)
    {
        if (byteLength < 1 || byteLength > int.MaxValue / 2)
            throw new ArgumentOutOfRangeException(nameof(byteLength), byteLength, "Fixed hexadecimal byte length must be greater than zero.");
        _byteLength = byteLength;
    }

    /// <inheritdoc />
    public void Encode(BufferWriter writer, string value, SerializationContext context)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        var bytes = Convert.FromHexString(value);
        context.ValidateStringLength(value.Length, typeof(string));
        if (bytes.Length != _byteLength)
            throw new ArgumentException($"Hexadecimal value must represent exactly {_byteLength} bytes.", nameof(value));
        writer.Write(bytes);
    }

    /// <inheritdoc />
    public string Decode(ref BufferReader reader, SerializationContext context)
    {
        context.ValidateStringLength(_byteLength * 2, typeof(string));
        return Convert.ToHexString(reader.ReadSpan(_byteLength));
    }
}
