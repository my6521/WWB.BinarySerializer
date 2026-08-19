namespace WWB.BinarySerializer.Codecs.Time;

/// <summary>提供标准时间 Codec 的注册扩展。</summary>
public static class SerializerBuilderExtensions
{
    /// <summary>注册所有标准时间 Value Codec。</summary>
    public static SerializerBuilder AddTimeCodecs(this SerializerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder
            .AddValueCodec(BcdDateTimeValueCodec.CodecName, new BcdDateTimeValueCodec())
            .AddValueCodec(BcdTimeSpanValueCodec.CodecName, new BcdTimeSpanValueCodec())
            .AddValueCodec(Cp56Time2aValueCodec.CodecName, new Cp56Time2aValueCodec())
            .AddValueCodec(UnixTimeSecondsValueCodec.CodecName, new UnixTimeSecondsValueCodec());
    }
}
