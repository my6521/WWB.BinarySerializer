using WWB.BinarySerializer.Attributes;
using WWB.BinarySerializer.Codecs.Text;
using WWB.BinarySerializer.Codecs.Time;

namespace WWB.BinarySerializer.WebApi.Sample.Models;

/// <summary>演示内置类型以及全部 Text、Time Codec 的设备报文。</summary>
[BinaryContract(EndianType = EndianType.Big)]
public sealed class DevicePacket
{
    /// <summary>获取或设置设备编号。</summary>
    [BinaryField(1)]
    public int Id { get; set; }

    /// <summary>获取或设置使用严格 ASCII 编码的设备代码。</summary>
    [BinaryField(2, ValueCodecName = LengthPrefixedAsciiStringValueCodec.CodecName)]
    public string DeviceCode { get; set; } = string.Empty;

    /// <summary>获取或设置表示二进制内容的十六进制字符串。</summary>
    [BinaryField(3, ValueCodecName = LengthPrefixedHexStringValueCodec.CodecName)]
    public string PayloadHex { get; set; } = string.Empty;

    /// <summary>获取或设置 CP56Time2a 格式的设备时间。</summary>
    [BinaryField(4, ValueCodecName = Cp56Time2aValueCodec.CodecName)]
    public DateTime DeviceTime { get; set; }

    /// <summary>获取或设置 BCD 格式的业务时间。</summary>
    [BinaryField(5, ValueCodecName = BcdDateTimeValueCodec.CodecName)]
    public DateTime BillingTime { get; set; }

    /// <summary>获取或设置无符号 32 位 Unix 秒数格式的创建时间。</summary>
    [BinaryField(6, ValueCodecName = UnixTimeSecondsValueCodec.CodecName)]
    public DateTime CreatedAt { get; set; }

    /// <summary>获取或设置 BCD HHmm 格式的日内时间。</summary>
    [BinaryField(7, ValueCodecName = BcdTimeSpanValueCodec.CodecName)]
    public TimeSpan TimeOfDay { get; set; }

    /// <summary>获取或设置使用 UTF-8 编码的说明。</summary>
    [BinaryField(8, LengthPrefixSize = 2)]
    public string Description { get; set; } = string.Empty;

    /// <summary>获取或设置采样值。</summary>
    [BinaryField(9, LengthPrefixSize = 2)]
    public int[] Samples { get; set; } = Array.Empty<int>();
}
