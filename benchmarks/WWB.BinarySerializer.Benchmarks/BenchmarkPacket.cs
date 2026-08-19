using WWB.BinarySerializer.Attributes;
using WWB.BinarySerializer.Codecs.Text;
using WWB.BinarySerializer.Codecs.Time;

namespace WWB.BinarySerializer.Benchmarks;

/// <summary>覆盖基础字段、集合以及扩展 Value Codec 的基准报文。</summary>
[BinaryContract(EndianType = EndianType.Big)]
public sealed class BenchmarkPacket
{
    /// <summary>获取或设置报文编号。</summary>
    [BinaryField(1)]
    public int Id { get; set; }

    /// <summary>获取或设置 ASCII 设备代码。</summary>
    [BinaryField(2, ValueCodecName = LengthPrefixedAsciiStringValueCodec.CodecName)]
    public string DeviceCode { get; set; } = string.Empty;

    /// <summary>获取或设置十六进制载荷。</summary>
    [BinaryField(3, ValueCodecName = LengthPrefixedHexStringValueCodec.CodecName)]
    public string PayloadHex { get; set; } = string.Empty;

    /// <summary>获取或设置 CP56Time2a 时间。</summary>
    [BinaryField(4, ValueCodecName = Cp56Time2aValueCodec.CodecName)]
    public DateTime DeviceTime { get; set; }

    /// <summary>获取或设置 Unix 秒数时间。</summary>
    [BinaryField(5, ValueCodecName = UnixTimeSecondsValueCodec.CodecName)]
    public DateTime CreatedAt { get; set; }

    /// <summary>获取或设置说明文本。</summary>
    [BinaryField(6, LengthPrefixSize = 2)]
    public string Description { get; set; } = string.Empty;

    /// <summary>获取或设置采样值。</summary>
    [BinaryField(7, LengthPrefixSize = 2)]
    public int[] Samples { get; set; } = Array.Empty<int>();
}
