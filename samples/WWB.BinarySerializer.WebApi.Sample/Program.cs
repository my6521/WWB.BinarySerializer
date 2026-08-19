using WWB.BinarySerializer;
using WWB.BinarySerializer.Codecs.Text;
using WWB.BinarySerializer.Codecs.Time;
using WWB.BinarySerializer.WebApi.Sample.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(
    new SerializerBuilder()
        .Configure(new SerializerOptions
        {
            MaxPayloadLength = 1024 * 1024,
            MaxStringLength = 64 * 1024,
            MaxCollectionLength = 10_000,
            MaxDepth = 32,
            RequireCompletePayload = true
        })
        .AddTextCodecs()
        .AddTimeCodecs()
        .Build());

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    name = "WWB.BinarySerializer Web API Sample",
    endpoints = new[]
    {
        "POST /api/packets/encode",
        "POST /api/packets/decode"
    }
}));

app.MapPost("/api/packets/encode", (DevicePacket packet, SerializerRuntime serializer) =>
{
    try
    {
        var payload = serializer.Serialize(packet);
        return Results.File(payload, "application/octet-stream", "device-packet.bin");
    }
    catch (Exception exception) when (exception is SerializationException or FormatException or ArgumentException)
    {
        return Results.BadRequest(new { message = exception.Message });
    }
})
.Accepts<DevicePacket>("application/json")
.Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
.ProducesProblem(StatusCodes.Status400BadRequest);

app.MapPost("/api/packets/decode", async (
    HttpRequest request,
    SerializerRuntime serializer,
    CancellationToken cancellationToken) =>
{
    if (request.ContentLength is > 1024 * 1024)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status413PayloadTooLarge,
            title: "载荷过大",
            detail: "二进制载荷不能超过 1 MiB。");
    }

    var payload = await ReadPayloadAsync(request.Body, 1024 * 1024, cancellationToken);

    if (payload is null)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status413PayloadTooLarge,
            title: "载荷过大",
            detail: "二进制载荷不能超过 1 MiB。");
    }

    if (payload.Length == 0)
    {
        return Results.BadRequest(new { message = "请求体不能为空。" });
    }

    try
    {
        var packet = serializer.Deserialize<DevicePacket>(payload);
        return Results.Ok(packet);
    }
    catch (Exception exception) when (exception is SerializationException or FormatException or ArgumentException)
    {
        return Results.BadRequest(new { message = exception.Message });
    }
})
.Accepts<byte[]>("application/octet-stream")
.Produces<DevicePacket>()
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status413PayloadTooLarge);

app.Run();

static async Task<byte[]?> ReadPayloadAsync(
    Stream input,
    int maximumLength,
    CancellationToken cancellationToken)
{
    using var output = new MemoryStream();
    var chunk = new byte[81920];

    while (true)
    {
        var read = await input.ReadAsync(chunk, cancellationToken);
        if (read == 0)
        {
            return output.ToArray();
        }

        if (output.Length + read > maximumLength)
        {
            return null;
        }

        await output.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
    }
}
