using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using WWB.BinarySerializer.Codecs.Text;
using WWB.BinarySerializer.Codecs.Time;

namespace WWB.BinarySerializer.Benchmarks;

/// <summary>比较 WWB.BinarySerializer 与 System.Text.Json 的时间和内存开销。</summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class SerializerBenchmarks
{
    private SerializerRuntime _runtime = null!;
    private BenchmarkPacket _packet = null!;
    private byte[] _binaryPayload = null!;
    private byte[] _jsonPayload = null!;

    /// <summary>获取或设置采样数组元素数量。</summary>
    [Params(8, 256)]
    public int SampleCount { get; set; }

    /// <summary>构造 Runtime、测试对象和反序列化输入。</summary>
    [GlobalSetup]
    public void Setup()
    {
        _runtime = new SerializerBuilder()
            .AddTextCodecs()
            .AddTimeCodecs()
            .Build();

        _packet = new BenchmarkPacket
        {
            Id = 42,
            DeviceCode = "DEVICE-01",
            PayloadHex = "A10BFF",
            DeviceTime = new DateTime(2026, 8, 20, 14, 30, 15, 123),
            CreatedAt = new DateTime(2026, 8, 20, 6, 30, 15, DateTimeKind.Utc),
            Description = "上海一号设备",
            Samples = Enumerable.Range(0, SampleCount).ToArray()
        };

        _binaryPayload = _runtime.Serialize(_packet);
        _jsonPayload = JsonSerializer.SerializeToUtf8Bytes(
            _packet,
            BenchmarkJsonContext.Default.BenchmarkPacket);
    }

    /// <summary>测试生成式二进制序列化。</summary>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Serialize")]
    public byte[] BinarySerialize() => _runtime.Serialize(_packet);

    /// <summary>测试 System.Text.Json 源生成序列化。</summary>
    [Benchmark]
    [BenchmarkCategory("Serialize")]
    public byte[] JsonSerialize() => JsonSerializer.SerializeToUtf8Bytes(
        _packet,
        BenchmarkJsonContext.Default.BenchmarkPacket);

    /// <summary>测试生成式二进制反序列化。</summary>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Deserialize")]
    public BenchmarkPacket BinaryDeserialize() =>
        _runtime.Deserialize<BenchmarkPacket>(_binaryPayload);

    /// <summary>测试 System.Text.Json 源生成反序列化。</summary>
    [Benchmark]
    [BenchmarkCategory("Deserialize")]
    public BenchmarkPacket JsonDeserialize() => JsonSerializer.Deserialize(
        _jsonPayload,
        BenchmarkJsonContext.Default.BenchmarkPacket)!;
}
