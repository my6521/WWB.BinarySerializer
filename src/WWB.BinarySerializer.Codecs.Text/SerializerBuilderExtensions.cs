namespace WWB.BinarySerializer.Codecs.Text;

/// <summary>提供标准文本 Codec 的注册扩展。</summary>
public static class SerializerBuilderExtensions
{
    /// <summary>注册长度前缀和固定长度的严格 ASCII Codec。</summary>
    public static SerializerBuilder AddAsciiCodecs(this SerializerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder
            .AddValueCodec(LengthPrefixedAsciiStringValueCodec.CodecName, new LengthPrefixedAsciiStringValueCodec())
            .AddValueCodec(FixedLengthAsciiStringValueCodec.CodecName, new FixedLengthAsciiStringValueCodec());
    }

    /// <summary>注册长度前缀和固定长度的严格十六进制字符串 Codec。</summary>
    public static SerializerBuilder AddHexCodecs(this SerializerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder
            .AddValueCodec(LengthPrefixedHexStringValueCodec.CodecName, new LengthPrefixedHexStringValueCodec())
            .AddValueCodec(FixedLengthHexStringValueCodec.CodecName, new FixedLengthHexStringValueCodec());
    }

    /// <summary>注册所有默认文本 Codec。</summary>
    public static SerializerBuilder AddTextCodecs(this SerializerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddAsciiCodecs().AddHexCodecs();
    }
}
