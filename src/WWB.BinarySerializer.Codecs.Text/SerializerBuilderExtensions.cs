namespace WWB.BinarySerializer.Codecs.Text;

/// <summary>提供标准文本 Codec 的注册扩展。</summary>
public static class SerializerBuilderExtensions
{
    /// <summary>注册默认的单字节长度前缀严格 ASCII Codec。</summary>
    public static SerializerBuilder AddAsciiCodec(this SerializerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddValueCodec(
            LengthPrefixedAsciiStringValueCodec.CodecName,
            new LengthPrefixedAsciiStringValueCodec());
    }

    /// <summary>注册默认的单字节长度前缀严格十六进制字符串 Codec。</summary>
    public static SerializerBuilder AddHexCodec(this SerializerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddValueCodec(
            LengthPrefixedHexStringValueCodec.CodecName,
            new LengthPrefixedHexStringValueCodec());
    }

    /// <summary>注册所有默认文本 Codec。</summary>
    public static SerializerBuilder AddTextCodecs(this SerializerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddAsciiCodec().AddHexCodec();
    }
}
