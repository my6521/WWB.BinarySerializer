using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer.Codecs.Text;

/// <summary>编码严格 ASCII 字符串，并写入可配置宽度的字节长度前缀。</summary>
public sealed class LengthPrefixedAsciiStringValueCodec : IValueCodec<string>
{
    /// <summary>获取标准注册名称。</summary>
    public const string CodecName = "ascii";
    private readonly int _lengthPrefixSize;

    /// <summary>使用 1 至 4 字节长度前缀初始化 Codec。</summary>
    public LengthPrefixedAsciiStringValueCodec(int lengthPrefixSize = 1)
    {
        if (lengthPrefixSize is < 1 or > sizeof(int))
            throw new ArgumentOutOfRangeException(nameof(lengthPrefixSize), lengthPrefixSize, "Length prefix size must be between 1 and 4 bytes.");
        _lengthPrefixSize = lengthPrefixSize;
    }

    /// <inheritdoc />
    public void Encode(BufferWriter writer, string value, SerializationContext context)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var bytes = AsciiEncoding.Encode(value);
        context.ValidateStringLength(bytes.Length, typeof(string));
        writer.WriteLength(bytes.Length, _lengthPrefixSize);
        writer.Write(bytes);
    }

    /// <inheritdoc />
    public string Decode(ref BufferReader reader, SerializationContext context)
    {
        var length = reader.ReadLength(_lengthPrefixSize);
        context.ValidateStringLength(length, typeof(string));
        return AsciiEncoding.Decode(reader.ReadSpan(length));
    }
}
