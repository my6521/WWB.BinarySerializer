namespace WWB.BinarySerializer.Runtime;

/// <summary>向 Value Codec 提供当前字段的线格式配置。</summary>
public readonly struct ValueCodecOptions
{
    /// <summary>获取未配置固定长度且使用单字节长度前缀的默认字段选项。</summary>
    public static ValueCodecOptions Default { get; } = new(0, 1);

    /// <summary>使用字段的固定长度和长度前缀宽度创建配置。</summary>
    /// <param name="fixedLength">固定长度；为 0 时表示未配置。</param>
    /// <param name="lengthPrefixSize">长度前缀占用的字节数。</param>
    public ValueCodecOptions(int fixedLength, int lengthPrefixSize)
    {
        if (fixedLength < 0)
            throw new ArgumentOutOfRangeException(nameof(fixedLength));
        if (lengthPrefixSize is < 1 or > sizeof(int))
            throw new ArgumentOutOfRangeException(nameof(lengthPrefixSize));
        FixedLength = fixedLength;
        LengthPrefixSize = lengthPrefixSize;
    }

    /// <summary>获取固定长度；为 0 时表示未配置。</summary>
    public int FixedLength { get; }

    /// <summary>获取长度前缀占用的字节数。</summary>
    public int LengthPrefixSize { get; }
}
