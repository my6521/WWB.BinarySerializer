# WWB.BinarySerializer.Codecs.Text

Strict text codecs for `WWB.BinarySerializer`.

```csharp
using WWB.BinarySerializer.Codecs.Text;

var runtime = new SerializerBuilder()
    .AddAsciiCodecs()
    .Build();

[BinaryField(1, ValueCodecName = LengthPrefixedAsciiStringValueCodec.CodecName)]
public string DeviceCode { get; set; } = string.Empty;
```

Use `AddHexCodecs()` for both length-prefixed and fixed-length Hex codecs, or `AddTextCodecs()` to register all ASCII and Hex codecs.

`LengthPrefixedAsciiStringValueCodec` uses the field's `LengthPrefixSize`, from one through four bytes. `FixedLengthAsciiStringValueCodec` uses the field's `FixedLength` and provides exact-length, prefix-free ASCII fields:

```csharp
[BinaryField(
    1,
    FixedLength = 8,
    ValueCodecName = FixedLengthAsciiStringValueCodec.CodecName)]
public string DeviceCode { get; set; } = string.Empty;
```

Both reject non-ASCII characters and bytes.

`LengthPrefixedHexStringValueCodec` and `FixedLengthHexStringValueCodec` encode hexadecimal text as binary bytes. For fixed Hex fields, `FixedLength` is the binary byte count, so `FixedLength = 2` requires four Hex characters. Odd-length input and non-hexadecimal characters are rejected with `FormatException`; decoded text is normalized to uppercase.
