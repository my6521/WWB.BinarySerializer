# WWB.BinarySerializer

[简体中文](README.md) | [English](README.en.md) | [Changelog](CHANGELOG.md)

A high-performance binary serialization library for .NET. Contract codecs are produced at compile time by a Source Generator, and the runtime reads and writes buffers directly without reflection-based scanning.

## Features

- Generates `IBinaryCodec<T>` implementations at compile time; primitive fields call `BufferReader` and `BufferWriter` directly
- Supports little-endian and big-endian byte order
- Supports primitive numbers, `bool`, `char`, enums, `DateTime`, `TimeSpan`, strings, arrays, `List<T>`, and nested contracts
- Supports fixed-length collections and length prefixes from 1 to 4 bytes
- Supports multiple named `IValueCodec<T>` registrations for the same CLR type
- Provides immutable, isolated `SerializerRuntime` configurations that are safe for concurrent reuse
- Enforces payload, string, collection, and nesting-depth limits
- Strictly validates UTF-8, ASCII, Hex, BCD, truncated payloads, and trailing data
- Ships Text and Time codecs as optional standalone NuGet packages

## Performance

The following local results were measured on .NET 10 with the default BenchmarkDotNet job. Source-generated `System.Text.Json` is used as the comparison baseline.

| Scenario | Binary | JSON | Binary relative performance | Binary allocated | JSON allocated |
|---|---:|---:|---:|---:|---:|
| Deserialize, 8 samples | 448.7 ns | 1,535.1 ns | 3.42x | 296 B | 848 B |
| Deserialize, 256 samples | 1,176.7 ns | 9,424.8 ns | 8.01x | 1,288 B | 3,944 B |
| Serialize, 8 samples | 510.1 ns | 615.5 ns | 1.21x | 264 B | 240 B |
| Serialize, 256 samples | 1,863.1 ns | 3,220.4 ns | 1.73x | 1,256 B | 1,136 B |

These results come from a particular machine and workload. They indicate relative trends in the current implementation and do not represent every application model. See [`benchmarks/WWB.BinarySerializer.Benchmarks`](benchmarks/WWB.BinarySerializer.Benchmarks) for the benchmark project, environment details, and reproduction commands.

## Requirements

- .NET 6.0 or later
- A C# compilation environment that supports Roslyn Source Generators

## Installation

The core package includes the Source Generator:

```powershell
dotnet add package WWB.BinarySerializer
```

Install optional codec packages as needed:

```powershell
dotnet add package WWB.BinarySerializer.Codecs.Text
dotnet add package WWB.BinarySerializer.Codecs.Time
```

## Quick start

Define a contract:

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

Serialize and deserialize:

```csharp
var runtime = SerializerRuntime.CreateDefault();

var payload = runtime.Serialize(new DevicePacket
{
    Id = 42,
    Name = "Device 1",
    Samples = new[] { 10, 20, 30 }
});

var packet = runtime.Deserialize<DevicePacket>(payload);
```

For simple scenarios, you can also use the process-wide default entry point:

```csharp
var payload = BinarySerializer.SerializeObject(packet);
var result = BinarySerializer.DeserializeObject<DevicePacket>(payload);
```

Prefer holding an explicit `SerializerRuntime` when you need isolated configuration, concurrent services, or custom codecs.

## Contract attributes

### BinaryContract

`BinaryContractAttribute` marks a class or record class for codec generation.

```csharp
[BinaryContract(EndianType = EndianType.Little)]
public sealed class Packet
{
}
```

`EndianType` controls the byte order of multibyte values and length prefixes. It defaults to little-endian.

`Size` is the initial capacity hint for the serialization buffer and defaults to `512`. It only affects allocation behavior, does not change the wire format, and is capped by the runtime payload limit.

### BinaryField

| Property | Meaning | Default |
|---|---|---:|
| `Order` | Field serialization order | `0` |
| `FixedLength` | Fixed length of a byte array, collection, or Value Codec field | `0`, meaning unspecified |
| `Ignore` | Excludes the field from serialization | `false` |
| `LengthPrefixSize` | Length-prefix size for a variable string, byte array, collection, or Value Codec | `1` |
| `ValueCodecName` | Named Value Codec used by a field or collection element | `null` |

Fields are sorted by `Order`. Fields with equal order retain their source declaration order.

Fixed-length example:

```csharp
[BinaryField(1, FixedLength = 6)]
public byte[] Address { get; set; } = new byte[6];
```

Variable-length example:

```csharp
[BinaryField(2, LengthPrefixSize = 2)]
public List<int> Values { get; set; } = new();
```

## Default wire format

- Integers, floating-point values, `decimal`, and enums use the contract byte order
- `bool` uses one byte: `0` or `1`
- `char` uses a 2-byte unsigned integer
- `DateTime` uses the 8-byte value produced by `DateTime.ToBinary()`
- `TimeSpan` uses its 8-byte `Ticks` value
- `string` uses strict UTF-8 with the encoded byte length as its prefix
- Arrays and `List<T>` write the element count followed by each encoded element
- Nested contracts resolve their `IBinaryCodec<T>` through the current runtime

Invalid UTF-8 bytes are rejected with a serialization exception instead of being replaced.

## Custom Value Codecs

An `IValueCodec<T>` defines one field-level wire format:

```csharp
using WWB.BinarySerializer.Buffers;
using WWB.BinarySerializer.Runtime;

public sealed class OffsetIntValueCodec : IValueCodec<int>
{
    public void Encode(
        BufferWriter writer,
        int value,
        SerializationContext context,
        ValueCodecOptions options) =>
        writer.WriteInt32(value + 10);

    public int Decode(
        ref BufferReader reader,
        SerializationContext context,
        ValueCodecOptions options) =>
        reader.ReadInt32() - 10;
}
```

Select a name on the field and register the same name in the runtime:

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

Registrations use a combination of CLR type and name. A single contract can therefore use the standard `DateTime` representation and multiple custom time formats:

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

## Integer wire codecs

Application properties can remain typed as `int` while a named Value Codec selects a narrower integer wire format. Standard integer codecs are built into `SerializerBuilder` and require no extra registration:

```csharp
var runtime = new SerializerBuilder().Build();
```

Fields and collections use the same codec names. A codec selected on a collection field is applied to every element:

```csharp
[BinaryField(1, ValueCodecName = Int32WireCodecs.UInt8)]
public int Status { get; set; }

[BinaryField(2, ValueCodecName = Int32WireCodecs.Int16BigEndian)]
public List<int> Measurements { get; set; } = new();
```

Available formats include `UInt8`, `Int8`, and little-endian or big-endian `UInt16`, `Int16`, `UInt24`, and `Int24`. Values outside the selected wire-format range throw `ArgumentOutOfRangeException` instead of being truncated. Explicit-endian codecs are not affected by the runtime's contract byte order. Use `ReplaceValueCodec()` to replace a built-in implementation.

## Text Codecs

Namespace:

```csharp
using WWB.BinarySerializer.Codecs.Text;
```

Register the default ASCII and Hex codecs together:

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

Available implementations:

- `LengthPrefixedAsciiStringValueCodec`
- `FixedLengthAsciiStringValueCodec`

ASCII codecs operate in strict mode. Non-ASCII characters and input bytes with the high bit set throw an exception instead of being replaced with `?`.

### Hex

```csharp
[BinaryField(
    1,
    ValueCodecName = LengthPrefixedHexStringValueCodec.CodecName)]
public string PayloadHex { get; set; } = string.Empty;
```

Available implementations:

- `LengthPrefixedHexStringValueCodec`
- `FixedLengthHexStringValueCodec`

`"ABCD"` is encoded as the bytes `AB CD`. The length prefix records the binary byte count, so the default variable-length representation is `02 AB CD`. Odd-length values, whitespace, and invalid hexadecimal characters throw `FormatException`. Decoded output is normalized to uppercase.

A fixed-length codec uses its registered name together with `FixedLength`:

```csharp
var runtime = new SerializerBuilder().AddHexCodecs().Build();

[BinaryField(
    1,
    FixedLength = 8,
    ValueCodecName = FixedLengthHexStringValueCodec.CodecName)]
public string Signature { get; set; } = string.Empty;
```

## Time Codecs

Namespace:

```csharp
using WWB.BinarySerializer.Codecs.Time;
```

Register all standard time codecs:

```csharp
var runtime = new SerializerBuilder()
    .AddTimeCodecs()
    .Build();
```

| Codec | Registration name | Wire format |
|---|---|---|
| `BcdDateTimeValueCodec` | `bcd-datetime` | 7-byte packed BCD `yyyyMMddHHmmss` |
| `BcdTimeSpanValueCodec` | `bcd-timespan` | 2-byte packed BCD `HHmm` |
| `Cp56Time2aValueCodec` | `cp56time2a` | IEC 60870-5 seven-byte CP56Time2a |
| `UnixTimeSecondsValueCodec` | `unix-time-seconds` | 4-byte unsigned Unix seconds |

Unix time can also be converted directly:

```csharp
var seconds = UnixTime.ToUInt32Seconds(DateTime.UtcNow);
var utc = UnixTime.FromUInt32Seconds(seconds);
```

Decoded values are always UTC. Values before the Unix epoch or outside the `uint` range throw `ArgumentOutOfRangeException`.

## Custom contract codecs

Implement `IBinaryCodec<T>` when a type cannot be expressed with attributes or requires full control over its wire format:

```csharp
public sealed class PacketCodec : IBinaryCodec<Packet>
{
    public void Encode(
        BufferWriter writer,
        Packet value,
        SerializationContext context)
    {
        // Custom writing
    }

    public Packet Decode(
        ref BufferReader reader,
        SerializationContext context)
    {
        // Custom reading
        return new Packet();
    }
}

var runtime = new SerializerBuilder()
    .AddCodec(new PacketCodec())
    .Build();
```

Use `ReplaceCodec` and `ReplaceValueCodec` to explicitly replace existing registrations. `Build()` creates an independent snapshot, so later changes to the builder do not affect an existing runtime.

## Safety limits

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

Default limits:

| Option | Default |
|---|---:|
| `MaxPayloadLength` | 16 MiB |
| `MaxStringLength` | 4 MiB |
| `MaxCollectionLength` | 1,000,000 |
| `MaxDepth` | 64 |
| `RequireCompletePayload` | `true` |

Disable `RequireCompletePayload` only when an external framing protocol explicitly manages the remaining bytes.

## Exceptions

- `SerializationException`: base serialization exception; may carry the contract type and byte offset
- `CodecNotFoundException`: a contract codec or named Value Codec is not registered
- `PayloadLimitExceededException`: the payload exceeds its configured limit
- `CollectionLimitExceededException`: a collection exceeds its configured element limit
- `TrailingDataException`: bytes remain after decoding completes

Individual codecs may also throw `FormatException`, `EncoderFallbackException`, `DecoderFallbackException`, or `ArgumentOutOfRangeException` for invalid input.

## Project structure

```text
src/
  WWB.BinarySerializer/
  WWB.BinarySerializer.Generator/
  WWB.BinarySerializer.Codecs.Text/
  WWB.BinarySerializer.Codecs.Time/
tests/
  WWB.BinarySerializer.RegressionTests.csproj
```

The core package does not depend on extension codec packages. The Text and Time packages depend only on the core package.

## Build and test

```powershell
dotnet restore WWB.BinarySerializer.sln
dotnet build WWB.BinarySerializer.sln -c Release --no-restore
dotnet test WWB.BinarySerializer.sln -c Release --no-build --no-restore
```

Package the projects:

```powershell
dotnet pack src/WWB.BinarySerializer/WWB.BinarySerializer.csproj -c Release --no-build --no-restore -o artifacts/packages
dotnet pack src/WWB.BinarySerializer.Codecs.Text/WWB.BinarySerializer.Codecs.Text.csproj -c Release --no-build --no-restore -o artifacts/packages
dotnet pack src/WWB.BinarySerializer.Codecs.Time/WWB.BinarySerializer.Codecs.Time.csproj -c Release --no-build --no-restore -o artifacts/packages
```

The Source Generator is included in the core NuGet package at:

```text
analyzers/dotnet/cs/WWB.BinarySerializer.Generator.dll
```

## Code and documentation conventions

- All text files use UTF-8 without BOM and CRLF line endings
- Public APIs have Chinese XML documentation comments
- Release builds must complete with zero warnings and zero errors
- New wire formats must include round-trip, exact-byte, boundary-value, and invalid-input tests
