namespace WWB.BinarySerializer.Runtime;

internal interface ICodecProvider
{
    bool TryGet<T>(out IBinaryCodec<T>? codec);
    bool TryGet(Type contractType, out object? codec);
}

internal sealed class ImmutableCodecProvider : ICodecProvider
{
    private readonly IReadOnlyDictionary<Type, object> _codecs;

    public ImmutableCodecProvider(IEnumerable<KeyValuePair<Type, object>> codecs)
    {
        _codecs = new Dictionary<Type, object>(codecs);
    }

    public bool TryGet<T>(out IBinaryCodec<T>? codec)
    {
        if (_codecs.TryGetValue(typeof(T), out var candidate) && candidate is IBinaryCodec<T> typed)
        {
            codec = typed;
            return true;
        }

        codec = null;
        return false;
    }

    public bool TryGet(Type contractType, out object? codec) => _codecs.TryGetValue(contractType, out codec);
}
