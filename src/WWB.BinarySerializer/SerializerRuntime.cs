using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer;

/// <summary>使用不可变 Codec 和选项快照提供线程安全的序列化能力。</summary>
public sealed class SerializerRuntime
{
    private readonly ICodecProvider _codecs;
    private readonly IValueCodecProvider _valueCodecs;

    internal SerializerRuntime(SerializerOptions options, ICodecProvider codecs, IValueCodecProvider valueCodecs)
    {
        Options = options;
        _codecs = codecs;
        _valueCodecs = valueCodecs;
    }

    /// <summary>获取该运行时已验证的选项。</summary>
    public SerializerOptions Options { get; }

    /// <summary>使用默认选项和生成的契约 Codec 创建运行时。</summary>
    public static SerializerRuntime CreateDefault() => new SerializerBuilder().Build();

    /// <summary>将值序列化到新分配的字节数组。</summary>
    public byte[] Serialize<T>(T value) where T : new()
    {
        ArgumentNullException.ThrowIfNull(value);
        var codec = ResolveCodec<T>();
        var writer = BufferWriter.CreatePooled(
            bigEndian: (codec as IEndianAwareCodec)?.BigEndian == true);
        try
        {
            var context = new SerializationContext(Options, _codecs, _valueCodecs);
            using (context.Enter(typeof(T))) codec.Encode(writer, value, context);
            EnsurePayloadWithinLimit<T>(writer.Length);
            return writer.ToArray();
        }
        finally
        {
            writer.Dispose();
        }
    }

    /// <summary>从字节数组反序列化值。</summary>
    public T Deserialize<T>(byte[] data) where T : new()
    {
        ArgumentNullException.ThrowIfNull(data);
        return Deserialize<T>(data.AsSpan());
    }

    /// <summary>从只读字节区间反序列化值。</summary>
    public T Deserialize<T>(ReadOnlySpan<byte> data) where T : new()
    {
        if (data.IsEmpty) throw new ArgumentException("载荷不能为空。", nameof(data));
        EnsurePayloadWithinLimit<T>(data.Length);
        var codec = ResolveCodec<T>();
        var reader = new BufferReader(data, (codec as IEndianAwareCodec)?.BigEndian == true);
        var context = new SerializationContext(Options, _codecs, _valueCodecs);
        try
        {
            T result;
            using (context.Enter(typeof(T))) result = codec.Decode(ref reader, context);
            if (Options.RequireCompletePayload && reader.Remaining != 0)
                throw new TrailingDataException(typeof(T), reader.Position, reader.Remaining);
            return result;
        }
        catch (SerializationException exception) when (exception.ContractType is null)
        {
            throw new SerializationException(exception.Message, typeof(T), exception.Offset, exception);
        }
    }

    private IBinaryCodec<T> ResolveCodec<T>() =>
        _codecs.TryGet<T>(out var local) && local is not null
            ? local
            : GeneratedCodecRegistry<T>.Instance ?? throw new CodecNotFoundException(typeof(T));

    private void EnsurePayloadWithinLimit<T>(int length)
    {
        if (length > Options.MaxPayloadLength)
            throw new PayloadLimitExceededException(typeof(T), length, Options.MaxPayloadLength);
    }
}
