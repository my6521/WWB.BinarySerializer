namespace WWB.BinarySerializer.Codecs.Time;

/// <summary>在 DateTime 值与无符号 32 位秒级 Unix 时间戳之间转换。</summary>
public static class UnixTime
{
    /// <summary>将日期时间转换为以整秒表示的无符号 Unix 时间戳。</summary>
    /// <exception cref="ArgumentOutOfRangeException">该值超出 UInt32 Unix 时间戳范围。</exception>
    public static uint ToUInt32Seconds(DateTime value)
    {
        var seconds = new DateTimeOffset(value.ToUniversalTime()).ToUnixTimeSeconds();
        if (seconds is < uint.MinValue or > uint.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), value, "该值超出 UInt32 Unix 时间戳范围。");
        return (uint)seconds;
    }

    /// <summary>将无符号秒级 Unix 时间戳转换为 UTC 日期时间。</summary>
    public static DateTime FromUInt32Seconds(uint seconds) =>
        DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
}
