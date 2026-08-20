namespace WWB.BinarySerializer.Attributes;

/// <summary>将属性标记为生成式二进制契约中的有序字段。</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class BinaryFieldAttribute : Attribute
{
    private int _fixedLength;
    private int _lengthPrefixSize = 1;
    private string? _valueCodecName;

    /// <summary>使用默认顺序初始化字段。</summary>
    public BinaryFieldAttribute()
    {
    }

    /// <summary>使用指定的序列化顺序初始化字段。</summary>
    /// <param name="order">字段的相对序列化顺序。</param>
    public BinaryFieldAttribute(int order)
    {
        Order = order;
    }

    /// <summary>获取字段的序列化顺序。</summary>
    public int Order { get; }

    /// <summary>获取或设置集合、字节序列或 Value Codec 字段的固定长度；零表示未配置。</summary>
    public int FixedLength
    {
        get => _fixedLength;
        set
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value), value, "FixedLength must be greater than zero.");
            _fixedLength = value;
        }
    }

    /// <summary>获取或设置是否从序列化中排除该字段。</summary>
    public bool Ignore { get; set; }

    /// <summary>获取或设置变长字段或 Value Codec 使用的长度前缀字节数。</summary>
    public int LengthPrefixSize
    {
        get => _lengthPrefixSize;
        set
        {
            if (value is < 1 or > sizeof(int))
                throw new ArgumentOutOfRangeException(nameof(value), value, "LengthPrefixSize must be between 1 and 4 bytes.");
            _lengthPrefixSize = value;
        }
    }

    /// <summary>获取或设置该字段或其集合元素使用的已注册 Value Codec 名称。</summary>
    public string? ValueCodecName
    {
        get => _valueCodecName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ValueCodecName cannot be empty or whitespace.", nameof(value));
            _valueCodecName = value;
        }
    }
}
