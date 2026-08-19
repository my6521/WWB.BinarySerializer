namespace WWB.BinarySerializer;

/// <summary>定义序列化运行时的资源限制和载荷验证行为。</summary>
public sealed class SerializerOptions
{
    /// <summary>获取默认序列化选项。</summary>
    public static SerializerOptions Default { get; } = new();

    /// <summary>获取允许序列化或反序列化的最大载荷字节数。</summary>
    public int MaxPayloadLength { get; init; } = 16 * 1024 * 1024;
    /// <summary>获取集合允许包含的最大元素数量。</summary>
    public int MaxCollectionLength { get; init; } = 1_000_000;
    /// <summary>获取编码后字符串允许使用的最大字节数。</summary>
    public int MaxStringLength { get; init; } = 4 * 1024 * 1024;
    /// <summary>获取契约允许的最大嵌套深度。</summary>
    public int MaxDepth { get; init; } = 64;
    /// <summary>获取反序列化时是否拒绝未消费的尾随字节。</summary>
    public bool RequireCompletePayload { get; init; } = true;

    internal void Validate()
    {
        if (MaxPayloadLength <= 0) throw new ArgumentOutOfRangeException(nameof(MaxPayloadLength));
        if (MaxCollectionLength <= 0) throw new ArgumentOutOfRangeException(nameof(MaxCollectionLength));
        if (MaxStringLength <= 0) throw new ArgumentOutOfRangeException(nameof(MaxStringLength));
        if (MaxDepth <= 0) throw new ArgumentOutOfRangeException(nameof(MaxDepth));
    }
}
