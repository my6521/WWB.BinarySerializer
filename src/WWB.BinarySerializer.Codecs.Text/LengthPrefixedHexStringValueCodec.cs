using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer.Codecs.Text;

/// <summary>将十六进制字符串编码为字节，并写入可配置宽度的字节长度前缀。</summary>
public sealed class LengthPrefixedHexStringValueCodec : IValueCodec<string>
{
    /// <summary>获取标准注册名称。</summary>
    public const string CodecName = "hex";
    private readonly int _lengthPrefixSize;

    /// <summary>使用 1 至 4 字节长度前缀初始化 Codec。</summary>
    public LengthPrefixedHexStringValueCodec(int lengthPrefixSize = 1)
    {
        if (lengthPrefixSize is < 1 or > sizeof(int))
            throw new ArgumentOutOfRangeException(nameof(lengthPrefixSize), lengthPrefixSize, "Length prefix size must be between 1 and 4 bytes.");
        _lengthPrefixSize = lengthPrefixSize;
    }

    /// <inheritdoc />
    public void Encode(BufferWriter writer, string value, SerializationContext context)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        var bytes = Convert.FromHexString(value);
        context.ValidateStringLength(value.Length, typeof(string));
        writer.WriteLength(bytes.Length, _lengthPrefixSize);
        writer.Write(bytes);
    }

    /// <inheritdoc />
    public string Decode(ref BufferReader reader, SerializationContext context)
    {
        var byteLength = reader.ReadLength(_lengthPrefixSize);
        if (byteLength > context.Options.MaxStringLength / 2)
            throw new SerializationException($"Hexadecimal string length exceeds the configured limit {context.Options.MaxStringLength}.", typeof(string));
        context.ValidateStringLength(byteLength * 2, typeof(string));
        return Convert.ToHexString(reader.ReadSpan(byteLength));
    }
}
