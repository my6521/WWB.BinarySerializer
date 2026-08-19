# WWB.BinarySerializer Benchmarks

该项目使用 BenchmarkDotNet 测量生成式二进制序列化与反序列化的耗时和内存分配，并使用启用源生成的 `System.Text.Json` 作为对照。

## 当前结果

测试日期：2026-08-20。

```text
BenchmarkDotNet 0.14.0
.NET SDK 10.0.302
.NET Runtime 10.0.10, X64 RyuJIT AVX2
Windows 10 22H2
Job: DefaultJob
```

| 方法 | 采样数量 | 平均耗时 | 相对 Binary 耗时 | Gen0 | 单次分配 |
|---|---:|---:|---:|---:|---:|
| BinaryDeserialize | 8 | 448.7 ns | 1.00 | 0.0935 | 296 B |
| JsonDeserialize | 8 | 1,535.1 ns | 3.42 | 0.2689 | 848 B |
| BinaryDeserialize | 256 | 1,176.7 ns | 1.00 | 0.4101 | 1,288 B |
| JsonDeserialize | 256 | 9,424.8 ns | 8.01 | 1.2512 | 3,944 B |
| BinarySerialize | 8 | 510.1 ns | 1.00 | 0.0839 | 264 B |
| JsonSerialize | 8 | 615.5 ns | 1.21 | 0.0763 | 240 B |
| BinarySerialize | 256 | 1,863.1 ns | 1.00 | 0.3986 | 1,256 B |
| JsonSerialize | 256 | 3,220.4 ns | 1.73 | 0.3586 | 1,136 B |

在该模型下，Binary 反序列化快 3.42 至 8.01 倍且分配约为 JSON 的三分之一；Binary 序列化快 1.21 至 1.73 倍，分配量与 JSON 接近。结果依赖硬件、运行时、报文结构和数据规模，不应直接外推到其他业务模型。

## 运行

运行完整基准：

```powershell
dotnet run -c Release --project benchmarks/WWB.BinarySerializer.Benchmarks -- --filter "*SerializerBenchmarks*" --join
```

只运行二进制序列化相关方法：

```powershell
dotnet run -c Release --project benchmarks/WWB.BinarySerializer.Benchmarks -- --filter "*Binary*"
```

不要在调试器中运行。BenchmarkDotNet 会进行预热和多轮测量，结果写入仓库根目录的 `BenchmarkDotNet.Artifacts`。重点关注：

- `Mean`：每次操作的平均耗时
- `Ratio`：相对于同组二进制基线的耗时比例
- `Allocated`：每次操作分配的托管内存
- `Gen0`、`Gen1`：垃圾回收压力

基准包含 8 和 256 个整数两种报文尺寸。比较不同提交时，应使用相同的机器、电源模式、SDK 和后台负载。
