namespace WWB.BinarySerializer;

using WWB.BinarySerializer.Runtime;

/// <summary>构建配置相互隔离的不可变序列化运行时。</summary>
public sealed class SerializerBuilder
{
    private readonly Dictionary<Type, object> _codecs = new();
    private readonly Dictionary<(Type Type, string Name), object> _valueCodecs = new();
    private SerializerOptions _options = SerializerOptions.Default;

    /// <summary>创建已注册内置 Value Codec 的序列化构建器。</summary>
    public SerializerBuilder()
    {
        BuiltInIntegerValueCodecs.Register(this);
    }

    /// <summary>设置该构建器所创建运行时的安全和载荷选项。</summary>
    /// <param name="options">已验证的序列化选项。</param>
    /// <returns>当前构建器。</returns>
    public SerializerBuilder Configure(SerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        _options = options;
        return this;
    }

    /// <summary>添加契约 Codec，并拒绝重复注册。</summary>
    public SerializerBuilder AddCodec<T>(IBinaryCodec<T> codec)
    {
        ArgumentNullException.ThrowIfNull(codec);
        if (!_codecs.TryAdd(typeof(T), codec))
            throw new InvalidOperationException($"{typeof(T).FullName} 已在当前 Builder 中注册 Codec。");
        return this;
    }

    /// <summary>添加或替换契约 Codec。</summary>
    public SerializerBuilder ReplaceCodec<T>(IBinaryCodec<T> codec)
    {
        ArgumentNullException.ThrowIfNull(codec);
        _codecs[typeof(T)] = codec;
        return this;
    }

    /// <summary>添加具名字段 Value Codec，并拒绝重复的类型与名称组合。</summary>
    public SerializerBuilder AddValueCodec<T>(string name, IValueCodec<T> codec)
    {
        ValidateValueCodecName(name);
        ArgumentNullException.ThrowIfNull(codec);
        if (!_valueCodecs.TryAdd((typeof(T), name), codec))
            throw new InvalidOperationException($"{typeof(T).FullName} 已注册名为 '{name}' 的 Value Codec。");
        return this;
    }

    /// <summary>添加或替换具名字段 Value Codec。</summary>
    public SerializerBuilder ReplaceValueCodec<T>(string name, IValueCodec<T> codec)
    {
        ValidateValueCodecName(name);
        ArgumentNullException.ThrowIfNull(codec);
        _valueCodecs[(typeof(T), name)] = codec;
        return this;
    }

    private static void ValidateValueCodecName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value Codec name cannot be empty or whitespace.", nameof(name));
    }

    /// <summary>根据当前配置创建不可变运行时快照。</summary>
    public SerializerRuntime Build() => new(
        _options,
        new ImmutableCodecProvider(_codecs),
        new ImmutableValueCodecProvider(_valueCodecs));
}
