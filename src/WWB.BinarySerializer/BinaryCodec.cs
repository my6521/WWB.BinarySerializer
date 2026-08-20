using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer;

/// <summary>编码和解码完整的二进制契约。</summary>
/// <typeparam name="T">契约类型。</typeparam>
public interface IBinaryCodec<T>
{
    /// <summary>编码契约值。</summary>
    void Encode(BufferWriter writer, T value, SerializationContext context);
    /// <summary>解码契约值。</summary>
    T Decode(ref BufferReader reader, SerializationContext context);
}

/// <summary>公开契约 Codec 所需的字节序。</summary>
public interface IEndianAwareCodec
{
    /// <summary>获取 Codec 是否使用大端字节序。</summary>
    bool BigEndian { get; }
}

/// <summary>为序列化写入缓冲区提供不影响线格式的初始容量提示。</summary>
public interface IBufferCapacityHint
{
    /// <summary>获取建议的初始缓冲区容量（字节数）。</summary>
    int InitialCapacity { get; }
}

/// <summary>保存为契约类型注册的源生成 Codec。</summary>
public static class GeneratedCodecRegistry<T>
{
    private static IBinaryCodec<T>? _codec;
    /// <summary>获取已注册的生成式 Codec；未注册时返回空。</summary>
    public static IBinaryCodec<T>? Instance => Volatile.Read(ref _codec);

    /// <summary>为契约类型注册首个生成式 Codec。</summary>
    public static bool TryRegister(IBinaryCodec<T> codec)
    {
        ArgumentNullException.ThrowIfNull(codec);
        return Interlocked.CompareExchange(ref _codec, codec, null) is null;
    }

}

/// <summary>向 Codec 提供运行时选项、Codec 解析和嵌套操作验证。</summary>
public sealed class SerializationContext
{
    private int _depth;

    private readonly IValueCodecProvider _valueCodecs;
    private readonly ICodecProvider _codecs;

    internal SerializationContext(SerializerOptions options, ICodecProvider codecs, IValueCodecProvider valueCodecs)
    {
        Options = options;
        _codecs = codecs;
        _valueCodecs = valueCodecs;
    }

    /// <summary>获取当前生效的序列化选项。</summary>
    public SerializerOptions Options { get; }
    /// <summary>获取当前契约嵌套深度。</summary>
    public int Depth => _depth;

    /// <summary>解析指定类型的具名 Value Codec。</summary>
    public IValueCodec<T> GetValueCodec<T>(string name) =>
        !string.IsNullOrWhiteSpace(name) && _valueCodecs.TryGet<T>(name, out var codec) && codec is not null
            ? codec
            : throw new CodecNotFoundException(typeof(T), name);

    /// <summary>解析运行时覆盖项或生成式契约 Codec。</summary>
    public IBinaryCodec<T> GetCodec<T>() =>
        _codecs.TryGet<T>(out var codec) && codec is not null
            ? codec
            : GeneratedCodecRegistry<T>.Instance ?? throw new CodecNotFoundException(typeof(T));

    /// <summary>进入嵌套契约，并返回可恢复原深度的作用域。</summary>
    public Scope Enter(Type contractType)
    {
        if (_depth >= Options.MaxDepth)
            throw new SerializationException($"类型 {contractType.FullName} 超过最大嵌套深度 {Options.MaxDepth}。", contractType);
        _depth++;
        return new Scope(this);
    }

    /// <summary>在分配内存前验证已解码的集合长度。</summary>
    public void ValidateCollectionLength(int length, Type contractType)
    {
        if (length < 0)
            throw new SerializationException($"类型 {contractType.FullName} 的集合长度不能为负数。", contractType);
        if (length > Options.MaxCollectionLength)
            throw new CollectionLimitExceededException(contractType, length, Options.MaxCollectionLength);
    }

    /// <summary>验证编码或解码后的字符串长度。</summary>
    public void ValidateStringLength(int length, Type contractType)
    {
        ValidateLength(length, Options.MaxStringLength, "字符串", contractType);
    }

    private static void ValidateLength(int length, int maximum, string kind, Type contractType)
    {
        if (length < 0)
            throw new SerializationException($"类型 {contractType.FullName} 的{kind}长度不能为负数。", contractType);
        if (length > maximum)
            throw new SerializationException($"类型 {contractType.FullName} 的{kind}长度 {length} 超过限制 {maximum}。", contractType);
    }

    /// <summary>释放时恢复序列化深度。</summary>
    public readonly struct Scope : IDisposable
    {
        private readonly SerializationContext _context;
        internal Scope(SerializationContext context) => _context = context;
        /// <summary>离开关联的嵌套契约作用域。</summary>
        public void Dispose() => _context._depth--;
    }
}
