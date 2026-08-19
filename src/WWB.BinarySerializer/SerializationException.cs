namespace WWB.BinarySerializer;

/// <summary>表示二进制序列化失败，并可携带契约和字节偏移上下文。</summary>
public class SerializationException : Exception
{
    /// <summary>初始化序列化异常。</summary>
    public SerializationException(string message, Type? contractType = null, int? offset = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ContractType = contractType;
        Offset = offset;
    }

    /// <summary>获取与失败关联的契约类型（如果已知）。</summary>
    public Type? ContractType { get; }
    /// <summary>获取与失败关联的字节偏移量（如果已知）。</summary>
    public int? Offset { get; }
}

/// <summary>表示载荷超过配置的最大大小。</summary>
public sealed class PayloadLimitExceededException : SerializationException
{
    /// <summary>初始化载荷限制异常。</summary>
    public PayloadLimitExceededException(Type contractType, int actualLength, int maximumLength)
        : base($"类型 {contractType.FullName} 的载荷长度 {actualLength} 超过限制 {maximumLength}。", contractType)
    {
        ActualLength = actualLength;
        MaximumLength = maximumLength;
    }

    /// <summary>获取实际载荷长度。</summary>
    public int ActualLength { get; }
    /// <summary>获取配置的最大载荷长度。</summary>
    public int MaximumLength { get; }
}

/// <summary>表示所需的契约 Codec 或具名 Value Codec 未注册。</summary>
public sealed class CodecNotFoundException : SerializationException
{
    /// <summary>初始化契约 Codec 缺失异常。</summary>
    public CodecNotFoundException(Type contractType)
        : base($"未为类型 {contractType.FullName} 注册 Codec。", contractType) { }

    /// <summary>初始化具名 Value Codec 缺失异常。</summary>
    public CodecNotFoundException(Type contractType, string? name)
        : base($"未为类型 {contractType.FullName} 注册名为 '{name}' 的 Value Codec。", contractType)
    {
        CodecName = name;
    }

    /// <summary>获取请求的 Value Codec 名称（如果适用）。</summary>
    public string? CodecName { get; }
}

/// <summary>表示契约解码完成后仍有未消费字节。</summary>
public sealed class TrailingDataException : SerializationException
{
    /// <summary>初始化尾随数据异常。</summary>
    public TrailingDataException(Type contractType, int offset, int trailingLength)
        : base($"类型 {contractType.FullName} 解码完成后仍有 {trailingLength} 字节未消费。", contractType, offset)
    {
        TrailingLength = trailingLength;
    }

    /// <summary>获取未消费的字节数。</summary>
    public int TrailingLength { get; }
}

/// <summary>表示集合超过配置的元素数量限制。</summary>
public sealed class CollectionLimitExceededException : SerializationException
{
    /// <summary>初始化集合限制异常。</summary>
    public CollectionLimitExceededException(Type contractType, int actualLength, int maximumLength)
        : base($"类型 {contractType.FullName} 的集合长度 {actualLength} 超过限制 {maximumLength}。", contractType)
    {
        ActualLength = actualLength;
        MaximumLength = maximumLength;
    }

    /// <summary>获取实际集合长度。</summary>
    public int ActualLength { get; }
    /// <summary>获取配置的最大集合长度。</summary>
    public int MaximumLength { get; }
}
