namespace WWB.BinarySerializer.Codecs.Time;

internal static class PackedBcd
{
    public static byte Encode(int value, string fieldName)
    {
        if (value is < 0 or > 99)
            throw new ArgumentOutOfRangeException(fieldName, value, "A packed BCD byte supports values from 0 through 99.");

        return (byte)(((value / 10) << 4) | value % 10);
    }

    public static int Decode(byte value)
    {
        var high = value >> 4;
        var low = value & 0x0F;
        if (high > 9 || low > 9)
            throw new FormatException($"0x{value:X2} is not a valid packed BCD byte.");
        return high * 10 + low;
    }
}
