using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace WWB.BinarySerializer.Buffers;

/// <summary>将基础值和字节序列写入可自动扩容的内存缓冲区。</summary>
public sealed class BufferWriter
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly ArrayBufferWriter<byte> _buffer;
    private readonly bool _bigEndian;

    /// <summary>使用指定的初始容量和字节序初始化写入器。</summary>
    public BufferWriter(int initialCapacity = 256, bool bigEndian = false)
    {
        if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        _buffer = new ArrayBufferWriter<byte>(initialCapacity);
        _bigEndian = bigEndian;
    }

    /// <summary>获取已写入的字节数。</summary>
    public int Length => _buffer.WrittenCount;
    /// <summary>获取已写入字节的只读视图。</summary>
    public ReadOnlySpan<byte> WrittenSpan => _buffer.WrittenSpan;

    /// <summary>写入无符号字节。</summary>
    public void WriteByte(byte value)
    {
        _buffer.GetSpan(1)[0] = value;
        _buffer.Advance(1);
    }

    /// <summary>写入有符号 16 位整数。</summary>
    public void WriteInt16(short value)
    {
        var span = _buffer.GetSpan(sizeof(short));
        if (_bigEndian) BinaryPrimitives.WriteInt16BigEndian(span, value);
        else BinaryPrimitives.WriteInt16LittleEndian(span, value);
        _buffer.Advance(sizeof(short));
    }

    /// <summary>写入无符号 16 位整数。</summary>
    public void WriteUInt16(ushort value)
    {
        var span = _buffer.GetSpan(sizeof(ushort));
        if (_bigEndian) BinaryPrimitives.WriteUInt16BigEndian(span, value);
        else BinaryPrimitives.WriteUInt16LittleEndian(span, value);
        _buffer.Advance(sizeof(ushort));
    }

    /// <summary>写入有符号 32 位整数。</summary>
    public void WriteInt32(int value)
    {
        var span = _buffer.GetSpan(sizeof(int));
        if (_bigEndian) BinaryPrimitives.WriteInt32BigEndian(span, value);
        else BinaryPrimitives.WriteInt32LittleEndian(span, value);
        _buffer.Advance(sizeof(int));
    }

    /// <summary>写入无符号 32 位整数。</summary>
    public void WriteUInt32(uint value)
    {
        var span = _buffer.GetSpan(sizeof(uint));
        if (_bigEndian) BinaryPrimitives.WriteUInt32BigEndian(span, value);
        else BinaryPrimitives.WriteUInt32LittleEndian(span, value);
        _buffer.Advance(sizeof(uint));
    }

    /// <summary>写入有符号 64 位整数。</summary>
    public void WriteInt64(long value)
    {
        var span = _buffer.GetSpan(sizeof(long));
        if (_bigEndian) BinaryPrimitives.WriteInt64BigEndian(span, value);
        else BinaryPrimitives.WriteInt64LittleEndian(span, value);
        _buffer.Advance(sizeof(long));
    }

    /// <summary>写入无符号 64 位整数。</summary>
    public void WriteUInt64(ulong value)
    {
        var span = _buffer.GetSpan(sizeof(ulong));
        if (_bigEndian) BinaryPrimitives.WriteUInt64BigEndian(span, value);
        else BinaryPrimitives.WriteUInt64LittleEndian(span, value);
        _buffer.Advance(sizeof(ulong));
    }

    /// <summary>写入单精度浮点数。</summary>
    public void WriteSingle(float value) => WriteInt32(BitConverter.SingleToInt32Bits(value));
    /// <summary>写入双精度浮点数。</summary>
    public void WriteDouble(double value) => WriteInt64(BitConverter.DoubleToInt64Bits(value));

    /// <summary>使用四个 32 位组成部分写入 decimal 值。</summary>
    public void WriteDecimal(decimal value)
    {
        foreach (var part in decimal.GetBits(value)) WriteInt32(part);
    }

    /// <summary>写入带编码字节长度前缀的严格 UTF-8 字符串。</summary>
    public void WriteUtf8(string value, int lengthByteCount)
    {
        ArgumentNullException.ThrowIfNull(value);
        var byteCount = StrictUtf8.GetByteCount(value);
        WriteLength(byteCount, lengthByteCount);
        var destination = _buffer.GetSpan(byteCount);
        var written = StrictUtf8.GetBytes(value.AsSpan(), destination);
        _buffer.Advance(written);
    }

    /// <summary>写入不带长度前缀的字节序列。</summary>
    public void Write(ReadOnlySpan<byte> value)
    {
        value.CopyTo(_buffer.GetSpan(value.Length));
        _buffer.Advance(value.Length);
    }

    /// <summary>使用 1 至 4 个字节写入非负长度。</summary>
    public void WriteLength(int value, int byteCount)
    {
        if (byteCount < 1 || byteCount > sizeof(int)) throw new ArgumentOutOfRangeException(nameof(byteCount));
        var maximum = byteCount == sizeof(int) ? int.MaxValue : (1 << (byteCount * 8)) - 1;
        if (value < 0 || value > maximum) throw new ArgumentOutOfRangeException(nameof(value));
        var span = _buffer.GetSpan(byteCount);
        for (var i = 0; i < byteCount; i++)
        {
            var index = _bigEndian ? byteCount - i - 1 : i;
            span[index] = (byte)(value >> (i * 8));
        }
        _buffer.Advance(byteCount);
    }

    /// <summary>将已写入字节复制到新数组。</summary>
    public byte[] ToArray() => _buffer.WrittenSpan.ToArray();
}
