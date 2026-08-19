using System.Text;

namespace WWB.BinarySerializer.Codecs.Text;

internal static class AsciiEncoding
{
    public static byte[] Encode(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] > 0x7F)
                throw new EncoderFallbackException($"Character U+{(int)value[i]:X4} at index {i} cannot be encoded as ASCII.");
        }
        return Encoding.ASCII.GetBytes(value);
    }

    public static string Decode(ReadOnlySpan<byte> value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] > 0x7F)
                throw new DecoderFallbackException($"Byte 0x{value[i]:X2} at index {i} is not valid ASCII.");
        }
        return Encoding.ASCII.GetString(value);
    }
}
