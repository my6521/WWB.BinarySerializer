namespace WWB.BinarySerializer;

/// <summary>提供以较窄整数线格式编码 <see cref="int"/> 值时使用的稳定 Codec 名称。</summary>
public static class Int32WireCodecs
{
    /// <summary>无符号 8 位整数，范围为 0 到 255。</summary>
    public const string UInt8 = "int32-uint8";

    /// <summary>有符号 8 位整数，范围为 -128 到 127。</summary>
    public const string Int8 = "int32-int8";

    /// <summary>小端无符号 16 位整数，范围为 0 到 65535。</summary>
    public const string UInt16LittleEndian = "int32-uint16-le";

    /// <summary>大端无符号 16 位整数，范围为 0 到 65535。</summary>
    public const string UInt16BigEndian = "int32-uint16-be";

    /// <summary>小端有符号 16 位整数，范围为 -32768 到 32767。</summary>
    public const string Int16LittleEndian = "int32-int16-le";

    /// <summary>大端有符号 16 位整数，范围为 -32768 到 32767。</summary>
    public const string Int16BigEndian = "int32-int16-be";

    /// <summary>小端无符号 24 位整数，范围为 0 到 16777215。</summary>
    public const string UInt24LittleEndian = "int32-uint24-le";

    /// <summary>大端无符号 24 位整数，范围为 0 到 16777215。</summary>
    public const string UInt24BigEndian = "int32-uint24-be";

    /// <summary>小端有符号 24 位整数，范围为 -8388608 到 8388607。</summary>
    public const string Int24LittleEndian = "int32-int24-le";

    /// <summary>大端有符号 24 位整数，范围为 -8388608 到 8388607。</summary>
    public const string Int24BigEndian = "int32-int24-be";
}
