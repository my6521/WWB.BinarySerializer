namespace WWB.BinarySerializer.Attributes;

/// <summary>将类标记为二进制序列化契约。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class BinaryContractAttribute : Attribute
{
    private int _size = 512;

    /// <summary>获取或设置该契约的生成 Codec 所使用的字节序。</summary>
    public EndianType EndianType { get; set; }

    /// <summary>获取或设置写入缓冲区的初始容量提示（字节数）。</summary>
    /// <remarks>该值仅影响初始缓冲区分配，不改变线格式；运行时会将其限制在载荷上限以内。</remarks>
    public int Size
    {
        get => _size;
        set
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Size must be greater than zero.");
            _size = value;
        }
    }
}
