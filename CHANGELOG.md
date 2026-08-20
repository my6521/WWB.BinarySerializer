# 版本日志

本文件记录 WWB.BinarySerializer 各版本的重要变化。格式参考 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，版本号遵循[语义化版本](https://semver.org/lang/zh-CN/)。

## 未发布

### 文档

- 添加完整英文 README，并在中英文文档间提供语言切换入口。

## 1.0.3 - 2026-08-20

### 新增

- 新增 `ValueCodecOptions`，将字段的 `FixedLength` 和 `LengthPrefixSize` 配置传递给 Value Codec。
- 支持通过同一个 Codec 实例处理不同字段级长度配置。

### 变更

- Text Codec 改为使用字段配置，不再要求为每种长度创建单独的 Codec 实例。
- `BinaryContractAttribute.Size` 明确作为初始缓冲区容量提示，并受 Runtime 最大载荷限制。

## 1.0.2 - 2026-08-20

### 变更

- `SerializerBuilder` 默认注册标准整数线格式 Codec，无需显式调用扩展注册方法。
- 新增 `ReplaceValueCodec()`，用于显式替换内置或已有的具名 Value Codec。

## 1.0.1 - 2026-08-20

### 新增

- 新增保持 CLR 类型为 `int` 的窄整数线格式 Codec。
- 支持 `UInt8`、`Int8`，以及大小端 `UInt16`、`Int16`、`UInt24` 和 `Int24`。
- 新增范围校验，超出目标线格式范围的值会被拒绝而不是静默截断。

## 1.0.0 - 2026-08-20

### 新增

- 首次发布核心二进制序列化运行时与 Roslyn Source Generator。
- 支持大小端基础类型、字符串、数组、集合、枚举、日期时间和嵌套契约。
- 支持自定义 `IBinaryCodec<T>`、具名 `IValueCodec<T>` 和不可变 `SerializerRuntime` 配置快照。
- 新增 Text Codec 包，提供严格 ASCII 与 Hex 线格式。
- 新增 Time Codec 包，提供 BCD、CP56Time2a 和 Unix 时间格式。
- 新增载荷、字符串、集合、嵌套深度和尾随数据安全检查。
- 新增 ASP.NET Core Web API 示例和 BenchmarkDotNet 性能基准。
