using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

namespace WWB.BinarySerializer;

/// <summary>提供标准整数线格式 Codec 的注册扩展。</summary>
public static class IntegerSerializerBuilderExtensions
{
    /// <summary>注册全部将 <see cref="int"/> 映射为 8、16 或 24 位线格式的 Value Codec。</summary>
    /// <param name="builder">要添加 Codec 的序列化构建器。</param>
    /// <returns>当前序列化构建器。</returns>
    public static SerializerBuilder AddIntegerValueCodecs(this SerializerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder
            .AddValueCodec(Int32WireCodecs.UInt8, new Int32WireValueCodec(1, false, false))
            .AddValueCodec(Int32WireCodecs.Int8, new Int32WireValueCodec(1, true, false))
            .AddValueCodec(Int32WireCodecs.UInt16LittleEndian, new Int32WireValueCodec(2, false, false))
            .AddValueCodec(Int32WireCodecs.UInt16BigEndian, new Int32WireValueCodec(2, false, true))
            .AddValueCodec(Int32WireCodecs.Int16LittleEndian, new Int32WireValueCodec(2, true, false))
            .AddValueCodec(Int32WireCodecs.Int16BigEndian, new Int32WireValueCodec(2, true, true))
            .AddValueCodec(Int32WireCodecs.UInt24LittleEndian, new Int32WireValueCodec(3, false, false))
            .AddValueCodec(Int32WireCodecs.UInt24BigEndian, new Int32WireValueCodec(3, false, true))
            .AddValueCodec(Int32WireCodecs.Int24LittleEndian, new Int32WireValueCodec(3, true, false))
            .AddValueCodec(Int32WireCodecs.Int24BigEndian, new Int32WireValueCodec(3, true, true));
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

        public void Encode(BufferWriter writer, int value, SerializationContext context)
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

        public int Decode(ref BufferReader reader, SerializationContext context)
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
