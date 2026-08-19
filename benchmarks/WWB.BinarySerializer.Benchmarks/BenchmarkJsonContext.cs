using System.Text.Json.Serialization;

namespace WWB.BinarySerializer.Benchmarks;

/// <summary>为 JSON 对照测试提供源生成元数据。</summary>
[JsonSerializable(typeof(BenchmarkPacket))]
internal sealed partial class BenchmarkJsonContext : JsonSerializerContext;
