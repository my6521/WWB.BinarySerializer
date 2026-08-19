namespace WWB.BinarySerializer.Attributes;

/// <summary>将类标记为二进制序列化契约。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class BinaryContractAttribute : Attribute
{
    /// <summary>获取或设置该契约的生成 Codec 所使用的字节序。</summary>
    public EndianType EndianType { get; set; }

    /// <summary>获取或设置写入缓冲区的初始容量提示（字节数）。</summary>
    /// <remarks>该值保留用于后续生成代码的容量优化。</remarks>
    public int Size { get; set; } = 512;
}
