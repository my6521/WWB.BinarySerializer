using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer.Codecs.Text;

/// <summary>将十六进制字符串编码为字节，并写入可配置宽度的字节长度前缀。</summary>
public sealed class LengthPrefixedHexStringValueCodec : IValueCodec<string>
{
    /// <summary>获取标准注册名称。</summary>
    public const string CodecName = "hex";
    /// <inheritdoc />
    public void Encode(BufferWriter writer, string value, SerializationContext context, ValueCodecOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        context.ValidateStringLength(value.Length, typeof(string));
        var bytes = Convert.FromHexString(value);
        writer.WriteLength(bytes.Length, options.LengthPrefixSize);
        writer.Write(bytes);
    }

    /// <inheritdoc />
    public string Decode(ref BufferReader reader, SerializationContext context, ValueCodecOptions options)
    {
        var byteLength = reader.ReadLength(options.LengthPrefixSize);
        if (byteLength > context.Options.MaxStringLength / 2)
            throw new SerializationException($"Hexadecimal string length exceeds the configured limit {context.Options.MaxStringLength}.", typeof(string));
        context.ValidateStringLength(byteLength * 2, typeof(string));
        return Convert.ToHexString(reader.ReadSpan(byteLength));
    }
}
