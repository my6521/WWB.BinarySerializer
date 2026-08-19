# WWB.BinarySerializer.Codecs.Time

Time-related value codecs for `WWB.BinarySerializer`:

- `BcdDateTimeValueCodec`: seven packed BCD bytes in `yyyyMMddHHmmss` order.
- `BcdTimeSpanValueCodec`: two packed BCD bytes in `HHmm` order.
- `Cp56Time2aValueCodec`: seven-byte IEC 60870-5 CP56Time2a representation.
- `UnixTimeSecondsValueCodec`: unsigned 32-bit Unix timestamp in seconds.

```csharp
using WWB.BinarySerializer.Codecs.Time;

var runtime = new SerializerBuilder()
    .AddTimeCodecs()
    .Build();
```

Direct conversion is also available through `UnixTime.ToUInt32Seconds` and `UnixTime.FromUInt32Seconds`. Decoded values are UTC.
