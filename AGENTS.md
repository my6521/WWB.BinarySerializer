# WWB.BinarySerializer 开发约定

## 适用范围

本文件适用于整个仓库。修改子目录前，应同时遵守目标目录中更具体的 `AGENTS.md`（如后续新增）。

## 项目结构

```text
src/
  WWB.BinarySerializer/              核心运行时、Buffer、Attribute 与公开扩展接口
  WWB.BinarySerializer.Generator/    Roslyn Source Generator
  WWB.BinarySerializer.Codecs.Text/  ASCII 与 Hex 字符串 Codec
  WWB.BinarySerializer.Codecs.Time/  BCD、CP56Time2a 与 Unix 时间 Codec
tests/
  WWB.BinarySerializer.RegressionTests.csproj
```

所有可发布项目必须位于 `src/<ProjectName>/`，项目名、程序集名、根命名空间和 NuGet `PackageId` 应保持一致。

## 架构约束

- 契约使用 `[BinaryContract]`，字段使用 `[BinaryField]`。
- `BinaryFieldAttribute` 的公开配置为 `Order`、`FixedLength`、`Ignore`、`LengthPrefixSize` 和 `ValueCodecName`。
- 普通字段由 Source Generator 生成直接的 `BufferReader` / `BufferWriter` 调用。
- 不得把内置标量改成运行时查表或接口分派；生成的基础类型代码应保持零反射、零 Codec 查找。
- 完整对象扩展实现 `IBinaryCodec<T>`；字段线格式扩展实现 `IValueCodec<T>`。
- Value Codec 使用“CLR 类型 + 名称”组合注册。同一类型允许注册多个不同名称的 Codec。
- `SerializerRuntime` 是不可变配置快照，应支持并发复用；不得引入跨 Runtime 的可变 Codec 配置。
- 嵌套对象必须通过当前 `SerializationContext.GetCodec<T>()` 解析，保证 Runtime 隔离。
- 新增读取逻辑前必须先验证长度、集合数量和嵌套深度，避免不受控分配。
- 非法 UTF-8、ASCII、Hex、BCD 和协议载荷应直接抛出明确异常，不得静默替换或吞掉错误。
- 默认反序列化必须拒绝尾随数据；仅由显式配置允许外部分帧场景保留尾随字节。

## Codec 归属

- 通用序列化基础能力放在 `WWB.BinarySerializer`。
- ASCII、Hex 及其他文本线格式放在 `WWB.BinarySerializer.Codecs.Text`。
- 日期、时间、时间戳和时间协议格式放在 `WWB.BinarySerializer.Codecs.Time`。
- 不要创建职责宽泛的 `Helpers`、`Utilities` 或 `Protocols` 杂项项目。
- 扩展包可以依赖核心包，核心包不得反向依赖任何扩展包。
- 新 Codec 应提供稳定的 `CodecName` 常量；适合默认批量注册时，应同步更新对应的 `SerializerBuilderExtensions`。

## Source Generator

- `BinarySerializerGenerator` 只负责发现契约、报告诊断和组装生成文件。
- 类型判定放在 `Emission/TypeShape.cs`，字段代码生成放在 `Emission/FieldEmitter.cs`。
- 重构生成器时必须检查实际生成代码，确保基础类型仍直接调用诸如 `WriteInt32`、`ReadInt32`、`DateTime.ToBinary` 等方法。
- 新增或修改诊断时，同步更新 `AnalyzerReleases.Shipped.md` 或 `AnalyzerReleases.Unshipped.md`。
- Generator 目标框架保持 `netstandard2.0`，以保证编译器宿主兼容性。

## API 与文档

- 公开 API 必须提供中文 XML 文档注释；`ASCII`、`UTF-8`、`Codec`、`DateTime` 等技术术语可保留英文。
- 修改公开 API 时同步更新根 `README.md`、扩展包 README 和回归测试。
- 公开命名应表达真实语义，禁止保留无行为或仅为旧版兼容而存在的属性。
- `BinaryContractAttribute.Size` 当前仅为预留容量提示，不得依赖它改变线格式。
- 线格式发生变化时必须增加精确字节断言测试，并在文档中说明破坏性影响。

## 文件格式

- 所有文本文件使用 UTF-8 无 BOM。
- 所有文本文件使用 CRLF 换行。
- 文件末尾保留一个换行。
- 遵守根目录 `.editorconfig` 和 `.gitattributes`，不要提交仅由错误换行或 BOM 引起的噪声差异。

## 构建与验证

在仓库根目录执行：

```powershell
dotnet restore WWB.BinarySerializer.sln
dotnet build WWB.BinarySerializer.sln -c Release --no-restore
dotnet test WWB.BinarySerializer.sln -c Release --no-build --no-restore
```

发布前还应验证三个包：

```powershell
dotnet pack src/WWB.BinarySerializer/WWB.BinarySerializer.csproj -c Release --no-build --no-restore -o artifacts/packages
dotnet pack src/WWB.BinarySerializer.Codecs.Text/WWB.BinarySerializer.Codecs.Text.csproj -c Release --no-build --no-restore -o artifacts/packages
dotnet pack src/WWB.BinarySerializer.Codecs.Time/WWB.BinarySerializer.Codecs.Time.csproj -c Release --no-build --no-restore -o artifacts/packages
```

核心 NuGet 包必须包含：

```text
analyzers/dotnet/cs/WWB.BinarySerializer.Generator.dll
```

## 测试要求

- 修复缺陷必须增加能够复现问题的回归测试。
- 新线格式至少覆盖正常往返、精确字节、边界值、截断和非法输入。
- 新具名 Codec 至少覆盖注册、缺失注册和同类型多名称共存。
- 涉及长度的功能必须覆盖 1 至 4 字节长度前缀、配置上限和溢出场景。
- 完成改动前要求 Release 构建 0 警告、0 错误，并通过全部测试。

## Git 约定

- 不要提交 `bin/`、`obj/`、`artifacts/` 或生成的 NuGet 包。
- 提交前运行 `git diff --check`，并检查暂存区没有混入无关文件。
- 提交信息使用简洁的英文 Conventional Commit 风格，例如 `refactor: organize projects under src`。

## 完成检查清单

1. 项目引用、解决方案路径和 CI 路径与当前目录结构一致。
2. 公开 API 具有中文 XML 文档。
3. Source Generator 生成路径未引入反射、运行时查表或额外分配。
4. Release 构建无警告、无错误。
5. 全部回归测试通过。
6. 所有发布包生成成功且包含预期文件。
7. 文本文件满足 UTF-8 无 BOM 与 CRLF。
