namespace WWB.BinarySerializer;

/// <summary>提供对进程级默认序列化运行时的便捷访问。</summary>
public static class BinarySerializer
{
    private static SerializerRuntime _default = SerializerRuntime.CreateDefault();

    /// <summary>获取当前默认运行时。</summary>
    public static SerializerRuntime Default => Volatile.Read(ref _default);

    /// <summary>以原子方式替换默认运行时。</summary>
    /// <param name="runtime">新的不可变运行时。</param>
    /// <returns>替换前的运行时。</returns>
    public static SerializerRuntime ReplaceDefault(SerializerRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        return Interlocked.Exchange(ref _default, runtime);
    }

    /// <summary>使用当前默认运行时序列化值。</summary>
    public static byte[] SerializeObject<T>(T value) where T : new() => Default.Serialize(value);

    /// <summary>使用当前默认运行时反序列化值。</summary>
    public static T DeserializeObject<T>(byte[] data) where T : new() => Default.Deserialize<T>(data);
}
