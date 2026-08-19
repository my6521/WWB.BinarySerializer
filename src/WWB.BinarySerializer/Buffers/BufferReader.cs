using System.Buffers.Binary;
using System.Text;

namespace WWB.BinarySerializer.Buffers;

/// <summary>从有界只读区间读取基础值和字节序列。</summary>
public ref struct BufferReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly ReadOnlySpan<byte> _buffer;
    private readonly bool _bigEndian;
    private int _position;

    /// <summary>使用指定缓冲区和字节序初始化读取器。</summary>
    public BufferReader(ReadOnlySpan<byte> buffer, bool bigEndian = false)
    {
        _buffer = buffer;
        _bigEndian = bigEndian;
        _position = 0;
    }

    /// <summary>获取当前字节偏移量。</summary>
    public int Position => _position;
    /// <summary>获取未读取的字节数。</summary>
    public int Remaining => _buffer.Length - _position;
    /// <summary>获取缓冲区中尚未读取的部分。</summary>
    public ReadOnlySpan<byte> RemainingSpan => _buffer.Slice(_position);

    /// <summary>读取无符号字节。</summary>
    public byte ReadByte()
    {
        EnsureAvailable(1);
        return _buffer[_position++];
    }

    /// <summary>读取有符号 16 位整数。</summary>
    public short ReadInt16()
    {
        var span = ReadSpan(sizeof(short));
        return _bigEndian ? BinaryPrimitives.ReadInt16BigEndian(span) : BinaryPrimitives.ReadInt16LittleEndian(span);
    }

    /// <summary>读取无符号 16 位整数。</summary>
    public ushort ReadUInt16()
    {
        var span = ReadSpan(sizeof(ushort));
        return _bigEndian ? BinaryPrimitives.ReadUInt16BigEndian(span) : BinaryPrimitives.ReadUInt16LittleEndian(span);
    }

    /// <summary>读取有符号 32 位整数。</summary>
    public int ReadInt32()
    {
        var span = ReadSpan(sizeof(int));
        return _bigEndian ? BinaryPrimitives.ReadInt32BigEndian(span) : BinaryPrimitives.ReadInt32LittleEndian(span);
    }

    /// <summary>读取无符号 32 位整数。</summary>
    public uint ReadUInt32()
    {
        var span = ReadSpan(sizeof(uint));
        return _bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(span) : BinaryPrimitives.ReadUInt32LittleEndian(span);
    }

    /// <summary>读取有符号 64 位整数。</summary>
    public long ReadInt64()
    {
        var span = ReadSpan(sizeof(long));
        return _bigEndian ? BinaryPrimitives.ReadInt64BigEndian(span) : BinaryPrimitives.ReadInt64LittleEndian(span);
    }

    /// <summary>读取无符号 64 位整数。</summary>
    public ulong ReadUInt64()
    {
        var span = ReadSpan(sizeof(ulong));
        return _bigEndian ? BinaryPrimitives.ReadUInt64BigEndian(span) : BinaryPrimitives.ReadUInt64LittleEndian(span);
    }

    /// <summary>读取单精度浮点数。</summary>
    public float ReadSingle() => BitConverter.Int32BitsToSingle(ReadInt32());
    /// <summary>读取双精度浮点数。</summary>
    public double ReadDouble() => BitConverter.Int64BitsToDouble(ReadInt64());

    /// <summary>从四个 32 位组成部分读取 decimal 值。</summary>
    public decimal ReadDecimal() => new(new[] { ReadInt32(), ReadInt32(), ReadInt32(), ReadInt32() });

    /// <summary>读取带长度前缀的严格 UTF-8 字符串。</summary>
    public string ReadUtf8(int lengthByteCount, SerializationContext context, Type contractType)
    {
        var byteCount = ReadLength(lengthByteCount);
        context.ValidateStringLength(byteCount, contractType);
        var offset = _position;
        try
        {
            return StrictUtf8.GetString(ReadSpan(byteCount));
        }
        catch (DecoderFallbackException exception)
        {
            throw new SerializationException("载荷包含无效的 UTF-8 字节序列。", contractType, offset, exception);
        }
    }

    /// <summary>读取指定数量的字节并推进当前位置。</summary>
    public ReadOnlySpan<byte> ReadSpan(int length)
    {
        EnsureAvailable(length);
        var result = _buffer.Slice(_position, length);
        _position += length;
        return result;
    }

    /// <summary>将当前位置向前推进指定字节数。</summary>
    public void Advance(int length)
    {
        EnsureAvailable(length);
        _position += length;
    }

    /// <summary>读取使用 1 至 4 个字节编码的无符号长度。</summary>
    public int ReadLength(int byteCount)
    {
        if (byteCount < 1 || byteCount > sizeof(int)) throw new ArgumentOutOfRangeException(nameof(byteCount));
        var bytes = ReadSpan(byteCount);
        uint value = 0;
        if (_bigEndian)
            for (var i = 0; i < byteCount; i++) value = (value << 8) | bytes[i];
        else
            for (var i = 0; i < byteCount; i++) value |= (uint)bytes[i] << (i * 8);
        if (value > int.MaxValue)
            throw new SerializationException($"长度前缀 {value} 超过 Int32 最大值。", offset: _position - byteCount);
        return (int)value;
    }

    private void EnsureAvailable(int length)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (length > Remaining)
            throw new SerializationException($"需要读取 {length} 字节，但仅剩余 {Remaining} 字节。", offset: _position);
    }
}
