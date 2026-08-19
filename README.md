# WWB.BinarySerializer

面向 .NET 的高性能二进制序列化库。契约 Codec 在编译期通过 Source Generator 生成，运行时直接读写缓冲区，不进行反射扫描。

## 特性

- 编译期生成 `IBinaryCodec<T>`，基础字段直接调用 `BufferReader` / `BufferWriter`
- 支持小端和大端字节序
- 支持基础数值、`bool`、`char`、枚举、`DateTime`、`TimeSpan`、字符串、数组、`List<T>` 和嵌套契约
- 支持定长集合与 1 至 4 字节长度前缀
- 支持同一 CLR 类型注册多个具名 `IValueCodec<T>`
- `SerializerRuntime` 配置不可变，可安全并发复用并相互隔离
- 提供载荷、字符串、集合和嵌套深度限制
- 严格验证 UTF-8、ASCII、Hex、BCD、截断载荷和尾随数据
- Text 与 Time Codec 通过独立 NuGet 包按需引用

## 性能

在 .NET 10、BenchmarkDotNet 默认 Job 下，以启用源生成的 `System.Text.Json` 为对照，本仓库基准模型得到以下本机结果：

| 场景 | Binary | JSON | Binary 相对性能 | Binary 分配 | JSON 分配 |
|---|---:|---:|---:|---:|---:|
| 反序列化，8 个采样值 | 448.7 ns | 1,535.1 ns | 3.42 倍 | 296 B | 848 B |
| 反序列化，256 个采样值 | 1,176.7 ns | 9,424.8 ns | 8.01 倍 | 1,288 B | 3,944 B |
| 序列化，8 个采样值 | 510.1 ns | 615.5 ns | 1.21 倍 | 264 B | 240 B |
| 序列化，256 个采样值 | 1,863.1 ns | 3,220.4 ns | 1.73 倍 | 1,256 B | 1,136 B |

结果来自特定机器和测试载荷，只用于观察当前实现的相对趋势，不代表所有业务模型。测试项目、环境信息和复现命令见 [`benchmarks/WWB.BinarySerializer.Benchmarks`](benchmarks/WWB.BinarySerializer.Benchmarks)。

## 环境要求

- .NET 6.0 或更高版本
- 支持 Roslyn Source Generator 的 C# 编译环境

## 安装

核心包已经包含 Source Generator：

```powershell
dotnet add package WWB.BinarySerializer
```

按需安装扩展 Codec：

```powershell
dotnet add package WWB.BinarySerializer.Codecs.Text
dotnet add package WWB.BinarySerializer.Codecs.Time
```

## 快速开始

定义契约：

```csharp
using WWB.BinarySerializer;
using WWB.BinarySerializer.Attributes;

[BinaryContract(EndianType = EndianType.Big)]
public sealed class DevicePacket
{
    [BinaryField(1)]
    public int Id { get; set; }

    [BinaryField(2, LengthPrefixSize = 2)]
    public string Name { get; set; } = string.Empty;

    [BinaryField(3, LengthPrefixSize = 2)]
    public int[] Samples { get; set; } = Array.Empty<int>();
}
```

序列化与反序列化：

```csharp
var runtime = SerializerRuntime.CreateDefault();

var payload = runtime.Serialize(new DevicePacket
{
    Id = 42,
    Name = "设备一",
    Samples = new[] { 10, 20, 30 }
});

var packet = runtime.Deserialize<DevicePacket>(payload);
```

简单场景也可以使用进程级默认入口：

```csharp
var payload = BinarySerializer.SerializeObject(packet);
var result = BinarySerializer.DeserializeObject<DevicePacket>(payload);
```

需要隔离配置、并发服务或自定义 Codec 时，优先显式持有 `SerializerRuntime`。

## 契约 Attribute

### BinaryContract

`BinaryContractAttribute` 标记需要生成 Codec 的类。

```csharp
[BinaryContract(EndianType = EndianType.Little)]
public sealed class Packet
{
}
```

`EndianType` 控制多字节数值和长度前缀的字节序，默认为小端。

### BinaryField

| 属性 | 含义 | 默认值 |
|---|---|---:|
| `Order` | 字段序列化顺序 | `0` |
| `FixedLength` | 字节数组或集合的固定长度 | `0`，表示写入长度前缀 |
| `Ignore` | 是否忽略该字段 | `false` |
| `LengthPrefixSize` | 变长字符串、字节数组或集合的长度前缀字节数 | `1` |
| `ValueCodecName` | 字段或集合元素使用的具名 Value Codec | `null` |

字段按 `Order` 排序；顺序相同时按源码声明位置稳定排序。

固定长度示例：

```csharp
[BinaryField(1, FixedLength = 6)]
public byte[] Address { get; set; } = new byte[6];
```

变长字段示例：

```csharp
[BinaryField(2, LengthPrefixSize = 2)]
public List<int> Values { get; set; } = new();
```

## 默认线格式

- 整数、浮点数、`decimal` 和枚举按照契约字节序写入
- `bool` 使用一个字节：`0` 或 `1`
- `char` 使用 2 字节无符号整数
- `DateTime` 使用 `DateTime.ToBinary()` 对应的 8 字节值
- `TimeSpan` 使用 8 字节 `Ticks`
- `string` 使用严格 UTF-8，并写入编码后的字节长度
- 数组和 `List<T>` 写入元素数量后逐项编码
- 嵌套契约通过当前 Runtime 解析对应 `IBinaryCodec<T>`

非法 UTF-8 字节不会被替换，将直接抛出序列化异常。

## 自定义 Value Codec

一个 `IValueCodec<T>` 负责一种字段线格式：

```csharp
using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

public sealed class OffsetIntValueCodec : IValueCodec<int>
{
    public void Encode(
        BufferWriter writer,
        int value,
        SerializationContext context) =>
        writer.WriteInt32(value + 10);

    public int Decode(
        ref BufferReader reader,
        SerializationContext context) =>
        reader.ReadInt32() - 10;
}
```

在字段上选择名称，并在 Runtime 中注册相同名称：

```csharp
[BinaryContract]
public sealed class CustomPacket
{
    [BinaryField(1, ValueCodecName = "offset")]
    public int Value { get; set; }
}

var runtime = new SerializerBuilder()
    .AddValueCodec("offset", new OffsetIntValueCodec())
    .Build();
```

注册键由“CLR 类型 + 名称”组成，因此同一个对象可以同时使用普通 `DateTime` 和多种自定义时间格式：

```csharp
[BinaryContract]
public sealed class TimePacket
{
    [BinaryField(1)]
    public DateTime CreatedAt { get; set; }

    [BinaryField(2, ValueCodecName = "cp56time2a")]
    public DateTime DeviceTime { get; set; }

    [BinaryField(3, ValueCodecName = "bcd-datetime")]
    public DateTime BillingTime { get; set; }
}
```

## Text Codecs

命名空间：

```csharp
using WWB.BinarySerializer.Codecs.Text;
```

一次注册默认 ASCII 和 Hex Codec：

```csharp
var runtime = new SerializerBuilder()
    .AddTextCodecs()
    .Build();
```

### ASCII

```csharp
[BinaryField(
    1,
    ValueCodecName = LengthPrefixedAsciiStringValueCodec.CodecName)]
public string DeviceCode { get; set; } = string.Empty;
```

可用实现：

- `LengthPrefixedAsciiStringValueCodec`
- `FixedLengthAsciiStringValueCodec`

ASCII Codec 使用严格模式。非 ASCII 字符和带高位的输入字节会直接抛出异常，不会替换为 `?`。

### Hex

```csharp
[BinaryField(
    1,
    ValueCodecName = LengthPrefixedHexStringValueCodec.CodecName)]
public string PayloadHex { get; set; } = string.Empty;
```

可用实现：

- `LengthPrefixedHexStringValueCodec`
- `FixedLengthHexStringValueCodec`

`"ABCD"` 会编码为字节 `AB CD`。长度前缀记录二进制字节数，因此默认变长格式为 `02 AB CD`。奇数长度、空格和非法十六进制字符会直接抛出 `FormatException`，解码结果统一为大写。

定长 Codec 需要使用自定义名称注册：

```csharp
var runtime = new SerializerBuilder()
    .AddValueCodec("hex-8", new FixedLengthHexStringValueCodec(8))
    .Build();
```

## Time Codecs

命名空间：

```csharp
using WWB.BinarySerializer.Codecs.Time;
```

一次注册全部标准时间 Codec：

```csharp
var runtime = new SerializerBuilder()
    .AddTimeCodecs()
    .Build();
```

| Codec | 注册名称 | 线格式 |
|---|---|---|
| `BcdDateTimeValueCodec` | `bcd-datetime` | 7 字节 `yyyyMMddHHmmss` 压缩 BCD |
| `BcdTimeSpanValueCodec` | `bcd-timespan` | 2 字节 `HHmm` 压缩 BCD |
| `Cp56Time2aValueCodec` | `cp56time2a` | IEC 60870-5 七字节 CP56Time2a |
| `UnixTimeSecondsValueCodec` | `unix-time-seconds` | 4 字节无符号 Unix 秒数 |

Unix 时间也可以直接转换：

```csharp
var seconds = UnixTime.ToUInt32Seconds(DateTime.UtcNow);
var utc = UnixTime.FromUInt32Seconds(seconds);
```

解码结果固定为 UTC。早于 Unix Epoch 或超过 `uint` 范围的值会直接抛出 `ArgumentOutOfRangeException`。

## 自定义完整契约 Codec

当类型无法通过 Attribute 表达，或需要完全控制对象线格式时，实现 `IBinaryCodec<T>`：

```csharp
public sealed class PacketCodec : IBinaryCodec<Packet>
{
    public void Encode(
        BufferWriter writer,
        Packet value,
        SerializationContext context)
    {
        // 自定义写入
    }

    public Packet Decode(
        ref BufferReader reader,
        SerializationContext context)
    {
        // 自定义读取
        return new Packet();
    }
}

var runtime = new SerializerBuilder()
    .AddCodec(new PacketCodec())
    .Build();
```

`ReplaceCodec` 和 `ReplaceValueCodec` 可用于显式替换已有注册。`Build()` 会创建独立快照，后续修改 Builder 不会影响已创建的 Runtime。

## 安全限制

```csharp
var runtime = new SerializerBuilder()
    .Configure(new SerializerOptions
    {
        MaxPayloadLength = 1024 * 1024,
        MaxStringLength = 64 * 1024,
        MaxCollectionLength = 100_000,
        MaxDepth = 32,
        RequireCompletePayload = true
    })
    .Build();
```

默认限制：

| 选项 | 默认值 |
|---|---:|
| `MaxPayloadLength` | 16 MiB |
| `MaxStringLength` | 4 MiB |
| `MaxCollectionLength` | 1,000,000 |
| `MaxDepth` | 64 |
| `RequireCompletePayload` | `true` |

只有外部分帧协议明确管理剩余字节时，才应关闭 `RequireCompletePayload`。

## 异常

- `SerializationException`：基础序列化异常，可携带契约类型和字节偏移
- `CodecNotFoundException`：契约 Codec 或具名 Value Codec 未注册
- `PayloadLimitExceededException`：载荷超过限制
- `CollectionLimitExceededException`：集合元素数量超过限制
- `TrailingDataException`：解码后仍存在未消费字节

具体 Codec 还会根据输入错误抛出 `FormatException`、`EncoderFallbackException`、`DecoderFallbackException` 或 `ArgumentOutOfRangeException`。

## 项目结构

```text
src/
  WWB.BinarySerializer/
  WWB.BinarySerializer.Generator/
  WWB.BinarySerializer.Codecs.Text/
  WWB.BinarySerializer.Codecs.Time/
tests/
  WWB.BinarySerializer.RegressionTests.csproj
```

核心包不依赖扩展 Codec 包。Text 和 Time 包只依赖核心包。

## 构建与测试

```powershell
dotnet restore WWB.BinarySerializer.sln
dotnet build WWB.BinarySerializer.sln -c Release --no-restore
dotnet test WWB.BinarySerializer.sln -c Release --no-build --no-restore
```

打包：

```powershell
dotnet pack src/WWB.BinarySerializer/WWB.BinarySerializer.csproj -c Release --no-build --no-restore -o artifacts/packages
dotnet pack src/WWB.BinarySerializer.Codecs.Text/WWB.BinarySerializer.Codecs.Text.csproj -c Release --no-build --no-restore -o artifacts/packages
dotnet pack src/WWB.BinarySerializer.Codecs.Time/WWB.BinarySerializer.Codecs.Time.csproj -c Release --no-build --no-restore -o artifacts/packages
```

核心 NuGet 包中的 Source Generator 位于：

```text
analyzers/dotnet/cs/WWB.BinarySerializer.Generator.dll
```

## 代码与文档规范

- 文本文件统一使用 UTF-8 无 BOM 与 CRLF
- 公开 API 使用中文 XML 文档注释
- Release 构建要求 0 警告、0 错误
- 新线格式必须提供正常往返、精确字节、边界值和非法输入测试
