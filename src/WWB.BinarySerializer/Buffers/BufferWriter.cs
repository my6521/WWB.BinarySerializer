using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace WWB.BinarySerializer.Buffers;

/// <summary>将基础值和字节序列写入可自动扩容的内存缓冲区。</summary>
public sealed class BufferWriter
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private byte[] _buffer;
    private readonly bool _bigEndian;
    private readonly bool _pooled;
    private int _writtenCount;
    private bool _disposed;

    /// <summary>使用指定的初始容量和字节序初始化写入器。</summary>
    public BufferWriter(int initialCapacity = 256, bool bigEndian = false)
        : this(initialCapacity, bigEndian, pooled: false)
    {
    }

    private BufferWriter(int initialCapacity, bool bigEndian, bool pooled)
    {
        if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        _buffer = pooled
            ? ArrayPool<byte>.Shared.Rent(Math.Max(initialCapacity, 1))
            : new byte[initialCapacity];
        _bigEndian = bigEndian;
        _pooled = pooled;
    }

    internal static BufferWriter CreatePooled(int initialCapacity = 256, bool bigEndian = false) =>
        new(initialCapacity, bigEndian, pooled: true);

    /// <summary>获取已写入的字节数。</summary>
    public int Length => _writtenCount;
    /// <summary>获取已写入字节的只读视图。</summary>
    public ReadOnlySpan<byte> WrittenSpan
    {
        get
        {
            ThrowIfDisposed();
            return _buffer.AsSpan(0, _writtenCount);
        }
    }

    /// <summary>写入无符号字节。</summary>
    public void WriteByte(byte value)
    {
        EnsureCapacity(1);
        _buffer[_writtenCount++] = value;
    }

    /// <summary>写入有符号 16 位整数。</summary>
    public void WriteInt16(short value)
    {
        var span = GetWritableSpan(sizeof(short));
        if (_bigEndian) BinaryPrimitives.WriteInt16BigEndian(span, value);
        else BinaryPrimitives.WriteInt16LittleEndian(span, value);
        _writtenCount += sizeof(short);
    }

    /// <summary>写入无符号 16 位整数。</summary>
    public void WriteUInt16(ushort value)
    {
        var span = GetWritableSpan(sizeof(ushort));
        if (_bigEndian) BinaryPrimitives.WriteUInt16BigEndian(span, value);
        else BinaryPrimitives.WriteUInt16LittleEndian(span, value);
        _writtenCount += sizeof(ushort);
    }

    /// <summary>写入有符号 32 位整数。</summary>
    public void WriteInt32(int value)
    {
        var span = GetWritableSpan(sizeof(int));
        if (_bigEndian) BinaryPrimitives.WriteInt32BigEndian(span, value);
        else BinaryPrimitives.WriteInt32LittleEndian(span, value);
        _writtenCount += sizeof(int);
    }

    /// <summary>写入无符号 32 位整数。</summary>
    public void WriteUInt32(uint value)
    {
        var span = GetWritableSpan(sizeof(uint));
        if (_bigEndian) BinaryPrimitives.WriteUInt32BigEndian(span, value);
        else BinaryPrimitives.WriteUInt32LittleEndian(span, value);
        _writtenCount += sizeof(uint);
    }

    /// <summary>写入有符号 64 位整数。</summary>
    public void WriteInt64(long value)
    {
        var span = GetWritableSpan(sizeof(long));
        if (_bigEndian) BinaryPrimitives.WriteInt64BigEndian(span, value);
        else BinaryPrimitives.WriteInt64LittleEndian(span, value);
        _writtenCount += sizeof(long);
    }

    /// <summary>写入无符号 64 位整数。</summary>
    public void WriteUInt64(ulong value)
    {
        var span = GetWritableSpan(sizeof(ulong));
        if (_bigEndian) BinaryPrimitives.WriteUInt64BigEndian(span, value);
        else BinaryPrimitives.WriteUInt64LittleEndian(span, value);
        _writtenCount += sizeof(ulong);
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
        var destination = GetWritableSpan(byteCount);
        var written = StrictUtf8.GetBytes(value.AsSpan(), destination);
        _writtenCount += written;
    }

    /// <summary>写入不带长度前缀的字节序列。</summary>
    public void Write(ReadOnlySpan<byte> value)
    {
        value.CopyTo(GetWritableSpan(value.Length));
        _writtenCount += value.Length;
    }

    /// <summary>使用 1 至 4 个字节写入非负长度。</summary>
    public void WriteLength(int value, int byteCount)
    {
        if (byteCount < 1 || byteCount > sizeof(int)) throw new ArgumentOutOfRangeException(nameof(byteCount));
        var maximum = byteCount == sizeof(int) ? int.MaxValue : (1 << (byteCount * 8)) - 1;
        if (value < 0 || value > maximum) throw new ArgumentOutOfRangeException(nameof(value));
        var span = GetWritableSpan(byteCount);
        for (var i = 0; i < byteCount; i++)
        {
            var index = _bigEndian ? byteCount - i - 1 : i;
            span[index] = (byte)(value >> (i * 8));
        }
        _writtenCount += byteCount;
    }

    /// <summary>将已写入字节复制到新数组。</summary>
    public byte[] ToArray() => WrittenSpan.ToArray();

    internal void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_pooled)
        {
            ArrayPool<byte>.Shared.Return(_buffer, clearArray: true);
            _buffer = Array.Empty<byte>();
            _writtenCount = 0;
        }
    }

    private Span<byte> GetWritableSpan(int sizeHint)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsSpan(_writtenCount, sizeHint);
    }

    private void EnsureCapacity(int sizeHint)
    {
        ThrowIfDisposed();
        if (sizeHint < 0) throw new ArgumentOutOfRangeException(nameof(sizeHint));
        if (sizeHint <= _buffer.Length - _writtenCount) return;

        var required = checked(_writtenCount + sizeHint);
        var newCapacity = Math.Max(required, Math.Max(_buffer.Length * 2, 256));
        var replacement = _pooled
            ? ArrayPool<byte>.Shared.Rent(newCapacity)
            : new byte[newCapacity];

        _buffer.AsSpan(0, _writtenCount).CopyTo(replacement);
        if (_pooled)
            ArrayPool<byte>.Shared.Return(_buffer, clearArray: true);
        _buffer = replacement;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(BufferWriter));
    }
}
