using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer;

internal static class BuiltInIntegerValueCodecs
{
    private static readonly IValueCodec<int> UInt8 = new Int32WireValueCodec(1, false, false);
    private static readonly IValueCodec<int> Int8 = new Int32WireValueCodec(1, true, false);
    private static readonly IValueCodec<int> UInt16LittleEndian = new Int32WireValueCodec(2, false, false);
    private static readonly IValueCodec<int> UInt16BigEndian = new Int32WireValueCodec(2, false, true);
    private static readonly IValueCodec<int> Int16LittleEndian = new Int32WireValueCodec(2, true, false);
    private static readonly IValueCodec<int> Int16BigEndian = new Int32WireValueCodec(2, true, true);
    private static readonly IValueCodec<int> UInt24LittleEndian = new Int32WireValueCodec(3, false, false);
    private static readonly IValueCodec<int> UInt24BigEndian = new Int32WireValueCodec(3, false, true);
    private static readonly IValueCodec<int> Int24LittleEndian = new Int32WireValueCodec(3, true, false);
    private static readonly IValueCodec<int> Int24BigEndian = new Int32WireValueCodec(3, true, true);

    public static void Register(SerializerBuilder builder)
    {
        builder
            .AddValueCodec(Int32WireCodecs.UInt8, UInt8)
            .AddValueCodec(Int32WireCodecs.Int8, Int8)
            .AddValueCodec(Int32WireCodecs.UInt16LittleEndian, UInt16LittleEndian)
            .AddValueCodec(Int32WireCodecs.UInt16BigEndian, UInt16BigEndian)
            .AddValueCodec(Int32WireCodecs.Int16LittleEndian, Int16LittleEndian)
            .AddValueCodec(Int32WireCodecs.Int16BigEndian, Int16BigEndian)
            .AddValueCodec(Int32WireCodecs.UInt24LittleEndian, UInt24LittleEndian)
            .AddValueCodec(Int32WireCodecs.UInt24BigEndian, UInt24BigEndian)
            .AddValueCodec(Int32WireCodecs.Int24LittleEndian, Int24LittleEndian)
            .AddValueCodec(Int32WireCodecs.Int24BigEndian, Int24BigEndian);
    }

    private sealed class Int32WireValueCodec : IValueCodec<int>
    {
        private readonly int _byteCount;
        private readonly bool _signed;
        private readonly bool _bigEndian;

        public Int32WireValueCodec(int byteCount, bool signed, bool bigEndian)
        {
            _byteCount = byteCount;
            _signed = signed;
            _bigEndian = bigEndian;
        }

        public void Encode(BufferWriter writer, int value, SerializationContext context, ValueCodecOptions options)
        {
            var bits = _byteCount * 8;
            var minimum = _signed ? -(1 << (bits - 1)) : 0;
            var maximum = _signed ? (1 << (bits - 1)) - 1 : (1 << bits) - 1;
            if (value < minimum || value > maximum)
                throw new ArgumentOutOfRangeException(nameof(value), value, $"值必须位于 {minimum} 到 {maximum} 之间。");

            for (var index = 0; index < _byteCount; index++)
            {
                var shift = (_bigEndian ? _byteCount - index - 1 : index) * 8;
                writer.WriteByte(unchecked((byte)(value >> shift)));
            }
        }

        public int Decode(ref BufferReader reader, SerializationContext context, ValueCodecOptions options)
        {
            var value = 0;
            for (var index = 0; index < _byteCount; index++)
            {
                var shift = (_bigEndian ? _byteCount - index - 1 : index) * 8;
                value |= reader.ReadByte() << shift;
            }

            var bits = _byteCount * 8;
            if (_signed && (value & (1 << (bits - 1))) != 0)
                value |= -1 << bits;
            return value;
        }
    }
}
