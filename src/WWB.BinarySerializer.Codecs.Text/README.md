# WWB.BinarySerializer.Codecs.Text

Strict text codecs for `WWB.BinarySerializer`.

```csharp
using WWB.BinarySerializer.Codecs.Text;

var runtime = new SerializerBuilder()
    .AddAsciiCodec()
    .Build();

[BinaryField(1, ValueCodecName = LengthPrefixedAsciiStringValueCodec.CodecName)]
public string DeviceCode { get; set; } = string.Empty;
```

`LengthPrefixedAsciiStringValueCodec` supports length prefixes from one through four bytes. `FixedLengthAsciiStringValueCodec` provides exact-length, prefix-free ASCII fields. Both reject non-ASCII characters and bytes.

`LengthPrefixedHexStringValueCodec` and `FixedLengthHexStringValueCodec` encode hexadecimal text as binary bytes. Odd-length input and non-hexadecimal characters are rejected with `FormatException`; decoded text is normalized to uppercase.
