namespace WWB.BinarySerializer;

/// <summary>指定多字节数值和长度前缀使用的字节序。</summary>
public enum EndianType
{
    /// <summary>低有效字节在前。</summary>
    Little = 0,

    /// <summary>高有效字节在前。</summary>
    Big = 1
}
