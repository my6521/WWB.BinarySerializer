using WWB.BinarySerializer.Buffers;

namespace WWB.BinarySerializer.Runtime;

/// <summary>使用具名的应用自定义线格式编码和解码字段值。</summary>
/// <typeparam name="T">字段或集合元素类型。</typeparam>
public interface IValueCodec<T>
{
    /// <summary>根据当前字段配置将值编码到目标写入器。</summary>
    void Encode(BufferWriter writer, T value, SerializationContext context, ValueCodecOptions options);

    /// <summary>根据当前字段配置从源读取器解码值。</summary>
    T Decode(ref BufferReader reader, SerializationContext context, ValueCodecOptions options);
}

internal interface IValueCodecProvider
{
    bool TryGet<T>(string name, out IValueCodec<T>? codec);
}

internal sealed class ImmutableValueCodecProvider : IValueCodecProvider
{
    private readonly IReadOnlyDictionary<(Type Type, string Name), object> _codecs;

    public ImmutableValueCodecProvider(IEnumerable<KeyValuePair<(Type Type, string Name), object>> codecs) =>
        _codecs = new Dictionary<(Type Type, string Name), object>(codecs);

    public bool TryGet<T>(string name, out IValueCodec<T>? codec)
    {
        if (_codecs.TryGetValue((typeof(T), name), out var candidate) && candidate is IValueCodec<T> typed)
        {
            codec = typed;
            return true;
        }

        codec = null;
        return false;
    }
}
