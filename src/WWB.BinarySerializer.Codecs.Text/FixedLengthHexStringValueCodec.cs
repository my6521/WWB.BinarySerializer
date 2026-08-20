using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer.Codecs.Text;

/// <summary>将十六进制字符串编码为字段配置的固定数量且不带前缀的字节。</summary>
public sealed class FixedLengthHexStringValueCodec : IValueCodec<string>
{
    /// <summary>获取固定长度 Hex Codec 的稳定注册名称。</summary>
    public const string CodecName = "hex-fixed";

    /// <inheritdoc />
    public void Encode(BufferWriter writer, string value, SerializationContext context, ValueCodecOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        var byteLength = GetFixedLength(options, context);
        context.ValidateStringLength(value.Length, typeof(string));
        if (value.Length != byteLength * 2)
            throw new ArgumentException($"Hexadecimal value must represent exactly {byteLength} bytes.", nameof(value));
        var bytes = Convert.FromHexString(value);
        writer.Write(bytes);
    }

    /// <inheritdoc />
    public string Decode(ref BufferReader reader, SerializationContext context, ValueCodecOptions options)
    {
        var byteLength = GetFixedLength(options, context);
        context.ValidateStringLength(byteLength * 2, typeof(string));
        return Convert.ToHexString(reader.ReadSpan(byteLength));
    }

    private static int GetFixedLength(ValueCodecOptions options, SerializationContext context)
    {
        if (options.FixedLength <= 0)
            throw new InvalidOperationException("Fixed-length Hex Codec requires BinaryField.FixedLength.");
        if (options.FixedLength > context.Options.MaxStringLength / 2)
            throw new SerializationException($"Hexadecimal string length exceeds the configured limit {context.Options.MaxStringLength}.", typeof(string));
        return options.FixedLength;
    }
}
