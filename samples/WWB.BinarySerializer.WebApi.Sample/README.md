# WWB.BinarySerializer Web API Sample

该项目演示在 ASP.NET Core Web API 中把 `SerializerRuntime` 注册为线程安全单例，并注册 Text 和 Time 包提供的全部标准 Codec。

运行：

```powershell
dotnet run --project samples/WWB.BinarySerializer.WebApi.Sample
```

服务默认监听 `http://localhost:5080`。

- `POST /api/packets/encode`：接收 JSON，返回 `application/octet-stream`
- `POST /api/packets/decode`：接收 `application/octet-stream`，返回 JSON

可以使用 `WWB.BinarySerializer.WebApi.Sample.http` 调用端点，也可以使用 curl：

```powershell
curl.exe -X POST http://localhost:5080/api/packets/encode `
  -H "Content-Type: application/json" `
  --data-binary "@packet.json" `
  --output device-packet.bin

curl.exe -X POST http://localhost:5080/api/packets/decode `
  -H "Content-Type: application/octet-stream" `
  --data-binary "@device-packet.bin"
```

ASCII 字段只能包含 ASCII 字符，Hex 字段必须包含偶数个十六进制字符。时间 Codec 也会严格检查各自格式的有效范围。
